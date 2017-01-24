using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration_Deprecated
{
    public class Debug_VisualiseCalibration : MonoBehaviour
    {
        private Quaternion _currentOvrRotation;

        void OnEnable()
        {
            SteamVR_Events.NewPoses.Listen(OnSteamVrPose);
        }

        void OnDisable()
        {
            SteamVR_Events.NewPoses.Remove(OnSteamVrPose);
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

            _currentOvrRotation = pose.rot;
        }


        void OnDrawGizmos()
        {
            // draw openvr's rotation
            Gizmos.color = Color.green;

            var start = transform.position;
            var end = start + (_currentOvrRotation * Vector3.forward);
            var up = start + (_currentOvrRotation * Vector3.up * 0.03f);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(start, up);

            // draw current (corrected) rotation
            Gizmos.color = Color.blue;
            start = transform.position;
            end = start + (transform.forward);
            up = start + (transform.up * 0.03f);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(start, up);
        }
    }
}
