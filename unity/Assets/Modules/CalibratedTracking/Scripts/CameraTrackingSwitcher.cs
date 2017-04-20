using Assets.Modules.Core.Animations;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class CameraTrackingSwitcher : MonoBehaviour
    {
        public ArMarkerCamTracker MarkerTracker { get; private set; }
        public DelayedVrCamTracker VrTracker { get; private set; }
        public OptitrackCamTracker OptitrackTracker { get; private set; }

        public bool TrackPosition = true;
        public bool TrackRotation = true;

        public enum TrackingMode { Optitrack, ArMarker, TransitionToArMarker, TransitionToOptitrack };
        public TrackingMode CurrentMode { get; private set; }

        private VectorAnimation _positionAnimation = new VectorAnimation(0.2f);
        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(0.2f);

        public CameraTrackingSwitcher()
        {
            MarkerTracker = new ArMarkerCamTracker();
            VrTracker = new DelayedVrCamTracker();
            OptitrackTracker = new OptitrackCamTracker();
        }

        private void Update()
        {
            MarkerTracker.PreparePose();
            VrTracker.PreparePose();
            OptitrackTracker.PreparePose();

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
            // TODO
            switch (CurrentMode)
            {
                case TrackingMode.Optitrack:
                    break;
                case TrackingMode.ArMarker:
                    break;
                case TrackingMode.TransitionToArMarker:
                    break;
                case TrackingMode.TransitionToOptitrack:
                    break;
            }

            ApplyTransform(MarkerTracker.GetPosition(), MarkerTracker.GetRotation());
        }

        private void UseOptitrack()
        {
            // TODO
            switch (CurrentMode)
            {
                case TrackingMode.Optitrack:
                    break;
                case TrackingMode.ArMarker:
                    break;
                case TrackingMode.TransitionToArMarker:
                    break;
                case TrackingMode.TransitionToOptitrack:
                    break;
            }

            ApplyTransform(OptitrackTracker.GetPosition(), VrTracker.GetRotation());
        }

        private void ApplyTransform(Vector3 pos, Quaternion rot)
        {
            if (TrackPosition)
                transform.position = pos;
            if (TrackRotation)
                transform.rotation = rot;
        }

    }
}
