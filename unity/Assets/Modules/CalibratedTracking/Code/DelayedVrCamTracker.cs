using Assets.Modules.Calibration;
using Assets.Modules.Tracking;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class DelayedVrCamTracker : CamTracker
    {
        public float TrackingDelay = 0.013f;

        private Queue<DelayedPose> _trackedPoses = new Queue<DelayedPose>();
        private Vector3 _position;
        private Quaternion _rotation;

        private class DelayedPose
        {
            public float TimeOfPose = 0f;
            public Vector3 Position = Vector3.zero;
            public Quaternion Rotation = Quaternion.identity;
        }

        public override void PreparePose()
        {
            StashPose();
            ApplyNewPose();
        }

        public override Vector3 GetPosition()
        {
            return _position;
        }

        public override Quaternion GetRotation()
        {
            return _rotation;
        }

        private void StashPose()
        {
            var pos = VRListener.CurrentPosition;
            var rot = VRListener.CurrentRotation;
            var time = VRListener.PoseUpdateTime;

            _trackedPoses.Enqueue(new DelayedPose
            {
                TimeOfPose = Time.time,
                Position = pos,
                Rotation = rot
            });
        }

        private void ApplyNewPose()
        {
            DelayedPose pose = null;

            var currentTime = Time.unscaledTime;
            while (_trackedPoses.Count > 0 && _trackedPoses.Peek().TimeOfPose + TrackingDelay < currentTime)
            {
                pose = _trackedPoses.Dequeue();
            }

            if (pose != null)
            {
                //_position = CalibrationParams.GetCalibratedPosition(pose.Position, pose.Rotation);
                _position = pose.Position; // position is calibrated 'against' Optitrack
                _rotation = CalibrationParams.GetCalibratedRotation(pose.Rotation);
            }
        }
    }
}