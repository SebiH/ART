using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Collections.Generic;

namespace Assets.Modules.Tracking
{
    public delegate void OptitrackPosesReceived(List<OptitrackPose> poses);

    public class OptitrackListener : MonoBehaviour
    {
        public static OptitrackListener Instance;

        public string IP = "127.0.0.1";
        public int Port = 16000;

        public event OptitrackPosesReceived PosesReceived;

        private IPEndPoint mRemoteIpEndPoint;
        private Socket mListener;
        private byte[] mReceiveBuffer;
        private string mPacket;
        private int mPreviousSubPacketIndex = 0;
        private const int kMaxSubPacketSize = 1400;

        void Awake()
        {
            Instance = this;

            mReceiveBuffer = new byte[kMaxSubPacketSize];
            mPacket = string.Empty;

            mRemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port);
            mListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mListener.Bind(mRemoteIpEndPoint);

            mListener.Blocking = false;
            mListener.ReceiveBufferSize = 128 * 1024;
        }

        public void UDPRead()
        {
            try
            {
                int bytesReceived = mListener.Receive(mReceiveBuffer);

                int maxSubPacketProcess = 200;

                while (bytesReceived > 0 && maxSubPacketProcess > 0)
                {
                    //== ensure header is present ==--
                    if (bytesReceived >= 2)
                    {
                        int subPacketIndex = mReceiveBuffer[0];
                        bool lastPacket = mReceiveBuffer[1] == 1;

                        if (subPacketIndex == 0)
                        {
                            mPacket = System.String.Empty;
                        }

                        if (subPacketIndex == 0 || subPacketIndex == mPreviousSubPacketIndex + 1)
                        {
                            mPacket += Encoding.ASCII.GetString(mReceiveBuffer, 2, bytesReceived - 2);

                            mPreviousSubPacketIndex = subPacketIndex;

                            if (lastPacket)
                            {
                                //== packet has been created from sub packets and is complete ==--
                                SendPacketNotification(mPacket);
                            }
                        }
                    }

                    bytesReceived = mListener.Receive(mReceiveBuffer);

                    //== time this out of packets are coming in faster than we can process ==--
                    maxSubPacketProcess--;
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void SendPacketNotification(string packet)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(packet);

            XmlNodeList rbList = xmlDoc.GetElementsByTagName("RigidBody");

            var poses = new List<OptitrackPose>();
    
            for (int index = 0; index < rbList.Count; index++)
            {
                var pose = new OptitrackPose();

                pose.Id = Convert.ToInt32(rbList[index].Attributes["ID"].InnerText);
                var rbName = rbList[index].Attributes["Name"].InnerText;
                pose.RigidbodyName = rbName;

                float x = (float)Convert.ToDouble(rbList[index].Attributes["x"].InnerText);
                float y = (float)Convert.ToDouble(rbList[index].Attributes["y"].InnerText);
                float z = (float)Convert.ToDouble(rbList[index].Attributes["z"].InnerText);

                //== coordinate system conversion (right to left handed) ==--
                z = -z;
                pose.Position = new Vector3(x, y, z);

                float qx = (float)Convert.ToDouble(rbList[index].Attributes["qx"].InnerText);
                float qy = (float)Convert.ToDouble(rbList[index].Attributes["qy"].InnerText);
                float qz = (float)Convert.ToDouble(rbList[index].Attributes["qz"].InnerText);
                float qw = (float)Convert.ToDouble(rbList[index].Attributes["qw"].InnerText);

                //== coordinate system conversion (right to left handed) ==--

                qz = -qz;
                qw = -qw;

                pose.Rotation = new Quaternion(qx, qy, qz, qw);

                for (var j = 0; j < rbList[index].ChildNodes.Count; j++)
                {
                    var marker = new OptitrackPose.Marker();
                    var markerXml = rbList[index].ChildNodes[j];

                    marker.Id = Convert.ToInt32(markerXml.Attributes["ID"].InnerText);

                    float mx = (float)Convert.ToDouble(markerXml.Attributes["x"].InnerText);
                    float my = (float)Convert.ToDouble(markerXml.Attributes["y"].InnerText);
                    float mz = (float)Convert.ToDouble(markerXml.Attributes["z"].InnerText);

                    //== coordinate system conversion (right to left handed) ==--
                    mz = -mz;

                    marker.Position = new Vector3(mx, my, mz);

                    pose.Markers.Add(marker);
                }

                poses.Add(pose);
            }

            if (PosesReceived != null)
            {
                PosesReceived(poses);
            }
        }

        void Update()
        {
            UDPRead();
        }
    }
}
