using Assets.Modules.Calibration;
using Assets.Modules.Tracking;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking.Scripts
{
    public class VrDelayedCalibratedTrackedObject : MonoBehaviour
    {
        public bool TrackRotation = true;
        public bool TrackPosition = false;

        [Range(0, 0.2f)]
        public float TrackingDelay = 0f;

        private Queue<DelayedPose> _trackedPoses = new Queue<DelayedPose>();

        private class DelayedPose
        {
            public float TimeOfPose = 0;
            public Vector3 Position = Vector3.zero;
            public Quaternion Rotation = Quaternion.identity;
        }

        private void Update()
        {
            StashPose();
            ApplyNewPose();
        }

        private void StashPose()
        {
            var pos = VRListener.Instance.CurrentPosition;
            var rot = VRListener.Instance.CurrentRotation;
            var time = VRListener.Instance.PoseUpdateTime;

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
                if (TrackPosition)
                {
                    transform.position = pose.Position + pose.Rotation * CalibrationParams.PositionOffset;
                }

                if (TrackRotation)
                {
                    transform.rotation = CalibrationParams.RotationOffset * pose.Rotation;
                }
            }
        }
    }
}
