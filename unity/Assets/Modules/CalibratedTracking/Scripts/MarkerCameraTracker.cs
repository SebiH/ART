using Assets.Modules.Calibration;
using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class MarkerCameraTracker : MonoBehaviour
    {
        // in seconds
        public float CutoffTime = 0.6f;
        public bool UseAnimationSmoothing = true;
        [Range(0.01f, 0.5f)]
        public float AnimationSmoothing = 0.1f;
        public bool IgnoreSmallChanges = true;

        // for debugging / gizmo drawing
        private bool _ignoredPosition = false;
        private bool _ignoredRotation = false;

        private VectorAnimation _positionAnimation = new VectorAnimation(0.1f);
        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(0.1f);

        private void OnEnable()
        {
            _positionAnimation.Init(transform.position);
            _rotationAnimation.Init(transform.rotation);
            _positionAnimation.AnimationSpeed = AnimationSmoothing;
            _rotationAnimation.AnimationSpeed = AnimationSmoothing;
        }

        private void Update()
        {
            var poses = ArMarkers.GetAll().Where(m => IsArMarkerValid(m));
            var position = MathUtility.Average(poses.Select(p => p.DetectedCameraPosition));
            var rotation = MathUtility.Average(poses.Select(p => p.DetectedCameraRotation));

#if UNITY_EDITOR
            _positionAnimation.AnimationSpeed = AnimationSmoothing;
            _rotationAnimation.AnimationSpeed = AnimationSmoothing;
#endif

            if (UseAnimationSmoothing)
            {
                RestartAnimations(position, rotation);

                transform.position = _positionAnimation.CurrentValue;
                transform.rotation = _rotationAnimation.CurrentValue;
            }
            else
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }

        private bool IsArMarkerValid(ArMarker marker)
        {
            return marker.HasDetectedCamera && Time.unscaledDeltaTime - marker.CameraDetectionTime < CutoffTime;
        }

        private void RestartAnimations(Vector3 position, Quaternion rotation)
        {
            _ignoredPosition = true;
            if (position != _positionAnimation.End)
            {
                if (!IgnoreSmallChanges || Mathf.Abs((transform.position - position).magnitude) > 0.02f)
                {
                    _positionAnimation.Restart(position);
                    _ignoredPosition = false;
                }
            }

            _ignoredRotation = true;
            if (rotation != _rotationAnimation.End)
            {
                if (!IgnoreSmallChanges || Quaternion.Angle(transform.rotation, rotation) > 1)
                {
                    _rotationAnimation.Restart(rotation);
                    _ignoredRotation = false;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _ignoredPosition ? Color.red : Color.green;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.color = _ignoredRotation ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.11f);
        }
    }
}
