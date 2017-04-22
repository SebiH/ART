using Assets.Modules.Calibration;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class ArMarkerCamTracker : CamTracker
    {
        // in seconds
        public float CutoffTime = 0.6f;
        public bool UseAverages = true;
        public float AverageWeight = 0.5f;

        private Vector3 _position;
        private Quaternion _rotation;
        private bool _hasPose;

        public bool HasPose()
        {
            return _hasPose;
        }

        public override void PreparePose()
        {
            var poses = ArMarkers.GetAll().Where(m => IsArMarkerValid(m));
            bool hadPose = _hasPose;
            _hasPose = poses.Count() > 0;

            if (!_hasPose)
            {
                return;
            }

            // weighted average - by marker confidence
            float x = 0, y = 0, z = 0;
            float qx = 0, qy = 0, qz = 0, qw = 0;

            foreach (var pose in poses)
            {
                var conf = pose.Confidence;
                x += conf * pose.DetectedCameraPosition.x;
                y += conf * pose.DetectedCameraPosition.y;
                z += conf * pose.DetectedCameraPosition.z;

                qx += conf * pose.DetectedCameraRotation.x;
                qy += conf * pose.DetectedCameraRotation.y;
                qz += conf * pose.DetectedCameraRotation.z;
                qw += conf * pose.DetectedCameraRotation.w;
            }

            float k = 1.0f / Mathf.Sqrt(qx * qx + qy * qy + qz * qz + qw * qw);
            var rot = new Quaternion(qx * k, qy * k, qz * k, qw * k);
            var pos = new Vector3(x, y, z) / poses.Count();

            if (UseAverages && hadPose)
            {
                qx = (AverageWeight * _rotation.x + (1 - AverageWeight) * rot.x);
                qy = (AverageWeight * _rotation.y + (1 - AverageWeight) * rot.y);
                qz = (AverageWeight * _rotation.z + (1 - AverageWeight) * rot.z);
                qw = (AverageWeight * _rotation.w + (1 - AverageWeight) * rot.w);
                _rotation = new Quaternion(qx, qy, qz, qw);
                _position = AverageWeight * _position + (1 - AverageWeight) * pos;
            }
            else
            {
                _position = pos;
                _rotation = rot;
            }
        }

        public override Vector3 GetPosition()
        {
            return _position;
        }

        public override Quaternion GetRotation()
        {
            return _rotation;
        }

        private bool IsArMarkerValid(ArMarker marker)
        {
            return marker.HasDetectedCamera && Time.unscaledDeltaTime - marker.CameraDetectionTime < CutoffTime;
        }
    }
}
