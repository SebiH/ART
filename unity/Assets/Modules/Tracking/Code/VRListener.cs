using Assets.Modules.Core;
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

        static VRListener()
        {
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
                var node = VRNode.Head;
                CurrentPosition = InputTracking.GetLocalPosition(node);
                CurrentRotation = InputTracking.GetLocalRotation(node);
                PoseUpdateTime = Time.unscaledTime;
            }
        }
    }
}
