using Assets.Modules.Tracking;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration
{
    public class Perpetual_Calibration : MonoBehaviour
    {
        public float ArCutoffTime = 1 / 30f;
        public float OptitrackCutoffTime = 1 / 30f;

        public string DisplayName = "Surface";

        // Camera used to determine which marker should be selected for calibration
        public Transform TrackedCamera;

        public string OptitrackCameraName = "HMD";
        private OptitrackPose _optitrackPose;
        private float _optitrackPoseTime;

        private Quaternion _ovrRotation;

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Utils.Event.Listen("new_poses", OnSteamVrPose);
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Utils.Event.Remove("new_poses", OnSteamVrPose);
        }


        void Update()
        {
            if (FixedDisplays.Has(DisplayName) && Time.unscaledTime - _optitrackPoseTime < OptitrackCutoffTime)
            {
                // get nearest marker (from center) with up-to-date ar marker pose
                MarkerPose nearestMarker = null;
                foreach (var marker in ArucoListener.Instance.DetectedPoses.Values)
                {
                    bool isCurrent = (Time.unscaledTime - marker.DetectionTime) < ArCutoffTime;
                    if (!isCurrent) { continue; } // shave off a few calculations
                    bool isNearest = (nearestMarker == null) || (false /* TODO compare distances */);

                    if (isCurrent && isNearest)
                    {
                        nearestMarker = marker;
                    }
                }

                if (nearestMarker != null)
                {
                    // apply calibration
                }
            }
        }


        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCameraName)
                {
                    _optitrackPose = pose;
                    _optitrackPoseTime = Time.unscaledTime;
                }
            }
        }

        private void OnSteamVrPose(params object[] args)
        {
            var i = (int)OpenVR.k_unTrackedDeviceIndex_Hmd;

            var poses = (TrackedDevicePose_t[])args[0];
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            _ovrRotation = pose.rot;
        }
    }
}
