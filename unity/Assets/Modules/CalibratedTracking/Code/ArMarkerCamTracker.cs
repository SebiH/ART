using Assets.Modules.Calibration;
using Assets.Modules.Core;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class ArMarkerCamTracker : CamTracker
    {
        // in seconds
        public float CutoffTime = 0.6f;

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
            _hasPose = poses.Count() > 0;

            _position = MathUtility.Average(poses.Select(p => p.DetectedCameraPosition));
            _rotation = MathUtility.Average(poses.Select(p => p.DetectedCameraRotation));
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
