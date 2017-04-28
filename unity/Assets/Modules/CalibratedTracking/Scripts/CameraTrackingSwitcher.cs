using Assets.Modules.Core.Animations;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class CameraTrackingSwitcher : MonoBehaviour
    {
        public ArMarkerCamTracker MarkerTracker { get; private set; }
        public DelayedVrCamTracker VrTracker { get; private set; }
        public OptitrackCamTracker OptitrackTracker { get; private set; }

        public float SwitchTransitionSpeed = 0.2f;

        public bool TrackPosition = true;
        public bool TrackRotation = true;

        public enum TrackingMode { Optitrack, ArMarker, TransitionToArMarker, TransitionToOptitrack };
        public TrackingMode CurrentMode = TrackingMode.Optitrack;

        private VectorAnimation _positionAnimation;
        private QuaternionAnimation _rotationAnimation;

        public CameraTrackingSwitcher()
        {
            MarkerTracker = new ArMarkerCamTracker();
            VrTracker = new DelayedVrCamTracker();
            OptitrackTracker = new OptitrackCamTracker();
            CurrentMode = TrackingMode.Optitrack;

            _positionAnimation = new VectorAnimation(SwitchTransitionSpeed);
            _rotationAnimation = new QuaternionAnimation(SwitchTransitionSpeed);

            _positionAnimation.Finished += AnimationFinished;
            _rotationAnimation.Finished += AnimationFinished;
        }

        private void OnEnable()
        {
            _positionAnimation.AnimationSpeed = SwitchTransitionSpeed;
            _rotationAnimation.AnimationSpeed = SwitchTransitionSpeed;
        }

        private void LateUpdate()
        {
            MarkerTracker.PreparePose();
            VrTracker.PreparePose();
            OptitrackTracker.PreparePose();

#if UNITY_EDITOR
            _positionAnimation.AnimationSpeed = SwitchTransitionSpeed;
            _rotationAnimation.AnimationSpeed = SwitchTransitionSpeed;
#endif

            if (MarkerTracker.HasPose())
            {
                UseArMarkers();
            }
            else
            {
                UseOptitrack();
            }
        }

        private void UseArMarkers()
        {
            var pos = MarkerTracker.GetPosition();
            var rot = MarkerTracker.GetRotation();

            switch (CurrentMode)
            {
                case TrackingMode.Optitrack:
                    CurrentMode = TrackingMode.TransitionToArMarker;
                    StartAnimations(pos, rot);
                    ApplyTransform(_positionAnimation.CurrentValue, _rotationAnimation.CurrentValue);
                    break;

                case TrackingMode.ArMarker:
                    ApplyTransform(pos, rot);
                    break;

                case TrackingMode.TransitionToArMarker:
                    AdjustAnimations(pos, rot);
                    ApplyTransform(_positionAnimation.CurrentValue, _rotationAnimation.CurrentValue);
                    break;

                case TrackingMode.TransitionToOptitrack:
                    CurrentMode = TrackingMode.TransitionToArMarker;
                    RestartAnimations(pos, rot);
                    ApplyTransform(_positionAnimation.CurrentValue, _rotationAnimation.CurrentValue);
                    break;
            }

        }

        private void UseOptitrack()
        {
            var pos = OptitrackTracker.GetPosition();
            var rot = VrTracker.GetRotation();

            switch (CurrentMode)
            {
                case TrackingMode.Optitrack:
                    ApplyTransform(pos, rot);
                    break;

                case TrackingMode.ArMarker:
                    CurrentMode = TrackingMode.TransitionToOptitrack;
                    StartAnimations(pos, rot);
                    ApplyTransform(_positionAnimation.CurrentValue, _rotationAnimation.CurrentValue);
                    break;

                case TrackingMode.TransitionToArMarker:
                    CurrentMode = TrackingMode.TransitionToOptitrack;
                    RestartAnimations(pos, rot);
                    ApplyTransform(_positionAnimation.CurrentValue, _rotationAnimation.CurrentValue);
                    break;

                case TrackingMode.TransitionToOptitrack:
                    AdjustAnimations(pos, rot);
                    ApplyTransform(_positionAnimation.CurrentValue, _rotationAnimation.CurrentValue);
                    break;
            }
        }

        private void ApplyTransform(Vector3 pos, Quaternion rot)
        {
            if (TrackPosition)
                transform.position = pos;
            if (TrackRotation)
                transform.rotation = rot;
        }

        private void StartAnimations(Vector3 targetPosition, Quaternion targetRotation)
        {
            _positionAnimation.Start(transform.position, targetPosition);
            _rotationAnimation.Start(transform.rotation, targetRotation);
        }

        private void RestartAnimations(Vector3 targetPosition, Quaternion targetRotation)
        {
            _positionAnimation.Restart(transform.position, targetPosition);
            _rotationAnimation.Restart(transform.rotation, targetRotation);
        }

        private void AdjustAnimations(Vector3 targetPosition, Quaternion targetRotation)
        {
            _positionAnimation.Adjust(targetPosition);
            _rotationAnimation.Adjust(targetRotation);
        }

        private void AnimationFinished()
        {
            if (CurrentMode == TrackingMode.TransitionToArMarker)
                CurrentMode = TrackingMode.ArMarker;
            else if (CurrentMode == TrackingMode.TransitionToOptitrack)
                CurrentMode = TrackingMode.Optitrack;
        }
    }
}
