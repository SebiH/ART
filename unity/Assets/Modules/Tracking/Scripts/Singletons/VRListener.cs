using Assets.Modules.Core;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;


namespace Assets.Modules.Tracking
{
    public class VRListener : MonoBehaviour
    {
        public static VRListener Instance { get; private set; }

        public Vector3 CurrentPosition { get; private set; }
        public Quaternion CurrentRotation { get; private set; }
        public float PoseUpdateTime { get; private set; }

        void OnEnable()
        {
            Instance = this;

            if (VrUtility.CurrentMode == VrUtility.VrMode.OpenVR)
            {
                // TODO: doesn't work..?
                SteamVR_Events.NewPoses.Listen(OnSteamVrPose);
            }
        }

        void OnDisable()
        {
            if (VrUtility.CurrentMode == VrUtility.VrMode.OpenVR)
            {
                // TODO: doesn't work..?
                SteamVR_Events.NewPoses.Remove(OnSteamVrPose);
            }
        }

        private void OnSteamVrPose(TrackedDevicePose_t[] poses)
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


        private void Update()
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
