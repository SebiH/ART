using Assets.Modules.Calibration;
using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class ArMarkerCamTracker : CamTracker
    {
        // in seconds
        public float CutoffTime = 0.6f;
        public bool IgnoreSmallChanges = true;
        public bool UseAnimationSmoothing = true;

        private float _animationSmoothing = 0.1f;
        public float AnimationSmoothing {
            get { return _animationSmoothing; }
            set
            {
                _animationSmoothing = value;
                _positionAnimation.AnimationSpeed = value;
                _rotationAnimation.AnimationSpeed = value;
            }
        }

        private VectorAnimation _positionAnimation;
        private QuaternionAnimation _rotationAnimation;

        // for debugging / gizmo drawing
        private bool _ignoredPosition = false;
        private bool _ignoredRotation = false;

        private Vector3 _position;
        private Quaternion _rotation;
        private bool _hasPose;

        public ArMarkerCamTracker()
        {
            _positionAnimation = new VectorAnimation(_animationSmoothing);
            _rotationAnimation = new QuaternionAnimation(_animationSmoothing);
        }

        public bool HasPose()
        {
            return _hasPose;
        }

        public override void PreparePose()
        {
            var poses = ArMarkers.GetAll().Where(m => IsArMarkerValid(m));
            var hadPose = _hasPose;
            _hasPose = poses.Count() > 0;

            var position = MathUtility.Average(poses.Select(p => p.DetectedCameraPosition));
            var rotation = MathUtility.Average(poses.Select(p => p.DetectedCameraRotation));

            if (!hadPose)
            {
                _positionAnimation.Init(position);
                _position = position;
                _rotationAnimation.Init(rotation);
                _rotation = rotation;
            }

            if (UseAnimationSmoothing)
            {
                RestartAnimations(_position, position, _rotation, rotation);

                _position = _positionAnimation.CurrentValue;
                _rotation = _rotationAnimation.CurrentValue;
            }
            else
            {
                _position = position;
                _rotation = rotation;
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

        private void RestartAnimations(Vector3 prevPosition, Vector3 position, Quaternion prevRotation, Quaternion rotation)
        {
            _ignoredPosition = true;
            if (position != _positionAnimation.End)
            {
                if (!IgnoreSmallChanges || Mathf.Abs((prevPosition - position).magnitude) > 0.02f)
                {
                    _positionAnimation.Restart(position);
                    _ignoredPosition = false;
                }
            }

            _ignoredRotation = true;
            if (rotation != _rotationAnimation.End)
            {
                if (!IgnoreSmallChanges || Quaternion.Angle(prevRotation, rotation) > 1)
                {
                    _rotationAnimation.Restart(rotation);
                    _ignoredRotation = false;
                }
            }
        }

        private bool IsArMarkerValid(ArMarker marker)
        {
            return marker.HasDetectedCamera && Time.unscaledDeltaTime - marker.CameraDetectionTime < CutoffTime;
        }

        public void DrawGizmos()
        {
            Gizmos.color = _ignoredPosition ? Color.red : Color.green;
            Gizmos.DrawSphere(_position, 0.1f);
            Gizmos.color = _ignoredRotation ? Color.red : Color.green;
            Gizmos.DrawWireSphere(_position, 0.11f);
        }
    }
}
