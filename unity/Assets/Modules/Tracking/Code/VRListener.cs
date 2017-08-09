using Assets.Modules.Core;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;


namespace Assets.Modules.Tracking
{
    public static class VRListener
    {
        public static Vector3 CurrentPosition { get; private set; }
        public static Quaternion CurrentRotation { get; private set; }
        public static float PoseUpdateTime { get; private set; }
        public static StreamWriter _cache;

        static VRListener()
        {
            _cache = File.AppendText(FileUtility.GetPath("test.json"));
            _cache.AutoFlush = true;
            _cache.WriteLine("[");

            if (VrUtility.CurrentMode == VrUtility.VrMode.OpenVR)
            {
                // TODO: doesn't work..?
                SteamVR_Events.NewPoses.Listen(OnSteamVrPose);
            }

            GameLoop.Instance.OnGameEnd += OnDisable;
            GameLoop.Instance.OnUpdate += Update;
        }

        private static void OnDisable()
        {
            if (VrUtility.CurrentMode == VrUtility.VrMode.OpenVR)
            {
                // TODO: doesn't work..?
                SteamVR_Events.NewPoses.Remove(OnSteamVrPose);
            }

            _cache.WriteLine("]");
            _cache.Close();
        }

        private static void OnSteamVrPose(TrackedDevicePose_t[] poses)
        {
            var i = (int)OpenVR.k_unTrackedDeviceIndex_Hmd;

            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            CurrentRotation = pose.rot;
            CurrentPosition = pose.pos;
            PoseUpdateTime = Time.unscaledTime;
        }


        private static void Update()
        {
            if (VrUtility.CurrentMode == VrUtility.VrMode.Native)
            {
                var node = VRNode.CenterEye;
                CurrentPosition = InputTracking.GetLocalPosition(node);
                CurrentRotation = InputTracking.GetLocalRotation(node);
                PoseUpdateTime = Time.unscaledTime;

                var pose = new VRPose
                {
                    Time = Time.unscaledTime,
                    HeadRot = InputTracking.GetLocalRotation(VRNode.Head),
                    HeadPos = InputTracking.GetLocalPosition(VRNode.Head),
                    LHRot = InputTracking.GetLocalRotation(VRNode.LeftHand),
                    LHPos = InputTracking.GetLocalPosition(VRNode.LeftHand),
                    RHRot = InputTracking.GetLocalRotation(VRNode.RightHand),
                    RHPos = InputTracking.GetLocalPosition(VRNode.RightHand),
                };
                _cache.WriteLine(JsonUtility.ToJson(pose));
            }
        }


        [Serializable]
        private struct VRPose
        {
            public float Time;
            public Quaternion HeadRot;
            public Vector3 HeadPos;
            public Quaternion LHRot;
            public Vector3 LHPos;
            public Quaternion RHRot;
            public Vector3 RHPos;
        }
    }
}
