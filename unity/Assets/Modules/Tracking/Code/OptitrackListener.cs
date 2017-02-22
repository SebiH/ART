using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Assets.Modules.Core;

namespace Assets.Modules.Tracking
{

    public static class OptitrackListener
    {
        public static event OptitrackPosesReceived PosesReceived;
        public delegate void OptitrackPosesReceived(List<OptitrackPose> poses);

        private static IPEndPoint mRemoteIpEndPoint;
        private static Socket mListener;
        private static byte[] mReceiveBuffer;
        private static string mPacket;
        private static int mPreviousSubPacketIndex = 0;
        private const int kMaxSubPacketSize = 1400;

        private static Dictionary<string, OptitrackPose> _detectedPoses = new Dictionary<string, OptitrackPose>(3);

        static OptitrackListener()
        {
            mReceiveBuffer = new byte[kMaxSubPacketSize];
            mPacket = string.Empty;

            mRemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Globals.OptitrackServerPort);
            mListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mListener.Bind(mRemoteIpEndPoint);

            mListener.Blocking = false;
            mListener.ReceiveBufferSize = 128 * 1024;

            GameLoop.Instance.OnGameEnd += OnDisable;
            GameLoop.Instance.OnUpdate += Update;
        }

        private static void OnDisable()
        {
            mListener.Close();
        }

        private static void Update()
        {
            UDPRead();
        }

        private static void UDPRead()
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

        private static void SendPacketNotification(string jsonPacket)
        {
            var packet = JsonUtility.FromJson<OptitrackPacket>(jsonPacket);
            var poses = new List<OptitrackPose>();

            foreach (var body in packet.Rigidbodies)
            {
                var pose = new OptitrackPose();

                pose.DetectionTime = Time.unscaledTime;
                pose.Id = body.Id;
                pose.RigidbodyName = body.Name;

                // right to left handed conversion
                pose.Position = new Vector3(body.X, body.Y, -body.Z) * Globals.OptitrackScaling; // z -> -z
                pose.Rotation = new Quaternion(body.QX, body.QY, -body.QZ, -body.QW); // qz -> -qz, qw -> -qw

                foreach (var marker in body.Markers)
                {
                    pose.Markers.Add(new OptitrackPose.Marker
                    {
                        Id = marker.Id,
                        // right to left handed conversion
                        Position = new Vector3(marker.X, marker.Y, -marker.Z) * Globals.OptitrackScaling
                    });
                }

                poses.Add(pose);
                _detectedPoses[body.Name] = pose;
            }

            if (PosesReceived != null)
            {
                PosesReceived(poses);
            }
        }

        public static bool HasPose(string name)
        {
            return _detectedPoses.ContainsKey(name);
        }

        public static OptitrackPose GetPose(string name)
        {
            if (HasPose(name))
            {
                return _detectedPoses[name];
            }
            return null;
        }

        #region Serialized classes

        [Serializable]
        private class OptitrackPacket
        {
            public OT_Rigidbody[] Rigidbodies = new OT_Rigidbody[] { };
        }

        [Serializable]
        private class OT_Rigidbody
        {
            public int Id = -1; 
            public string Name = "";

            public float X = 0;
            public float Y = 0;
            public float Z = 0;

            public float QX = 0;
            public float QY = 0;
            public float QZ = 0;
            public float QW = 0;

            public OT_Marker[] Markers = new OT_Marker[] { };
        }

        [Serializable]
        private class OT_Marker
        {
            public int Id = -1;

            public float X = 0;
            public float Y = 0;
            public float Z = 0;
        }

        #endregion
    }
}
