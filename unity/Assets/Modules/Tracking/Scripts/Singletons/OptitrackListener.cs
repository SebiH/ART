using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Assets.Modules.Tracking
{
    public delegate void OptitrackPosesReceived(List<OptitrackPose> poses);

    public class OptitrackListener : MonoBehaviour
    {
        private class OptitrackPacket
        {
            public OT_Rigidbody[] Rigidbodies;
        }

        private class OT_Rigidbody
        {
            public int Id;
            public string Name;

            public float X;
            public float Y;
            public float Z;

            public float QX;
            public float QY;
            public float QZ;
            public float QW;

            public OT_Marker[] Markers;
        }

        private class OT_Marker
        {
            public int Id;

            public float X;
            public float Y;
            public float Z;
        }



        public static OptitrackListener Instance;

        public string IP = "127.0.0.1";
        public int Port = 16000;
        public float Scaling = 0.8f;

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

        private void UDPRead()
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

        private void SendPacketNotification(string jsonPacket)
        {
            var packet = JsonUtility.FromJson<OptitrackPacket>(jsonPacket);
            var poses = new List<OptitrackPose>();

            foreach (var body in packet.Rigidbodies)
            {
                var pose = new OptitrackPose();

                pose.DetectionTime = Time.unscaledTime;
                pose.Id = body.Id;
                pose.RigidbodyName = body.Name;
                pose.Position = new Vector3(body.X, body.Y, body.Z);
                pose.Rotation = new Quaternion(body.QX, body.QY, body.QZ, body.QW);

                foreach (var marker in body.Markers)
                {
                    pose.Markers.Add(new OptitrackPose.Marker
                    {
                        Id = marker.Id,
                        Position = new Vector3(marker.X, marker.Y, marker.Z)
                    });
                }
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
