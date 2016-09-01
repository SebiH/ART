using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Tracking
{
    public class PerformCalibration : MonoBehaviour
    {
        public bool IsReadyForCalibration;

        void OnEnable()
        {
            IsReadyForCalibration = false;
            ArToolkitListener.Instance.NewPoseDetected += OnArtkPose;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Utils.Event.Listen("new_poses", OnSteamVrPose);
        }

        void OnDisable()
        {
            ArToolkitListener.Instance.NewPoseDetected -= OnArtkPose;
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Utils.Event.Remove("new_poses", OnSteamVrPose);
        }

        private Vector3 _position = Vector3.zero;
        private Quaternion _rotation = Quaternion.identity;

        private void OnArtkPose(MarkerPose pose)
        {
            _position = pose.Position;
            //transform.rotation = Quaternion.Inverse(rotation);
            _rotation = pose.Rotation;
        }

        void Update()
        {
            transform.position = _position;
            transform.rotation = _rotation;
        }

        private void OnOptitrackPose(List<OptitrackPose> poses)
        {

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

            //(pose);
        }


        public void Calibrate()
        {

        }
    }
}