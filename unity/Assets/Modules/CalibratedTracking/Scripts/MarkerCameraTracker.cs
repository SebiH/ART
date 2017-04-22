using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    [RequireComponent(typeof(CameraTrackingSwitcher))]
    public class MarkerCameraTracker : MonoBehaviour
    {
        // in seconds
        public float CutoffTime = 0.6f;
        public bool UseAverage = true;
        public float AverageWeight = 0.5f;

        public bool TrackPosition = false;
        public bool TrackRotation = false;

        private CameraTrackingSwitcher _camTracker;

        private void OnEnable()
        {
            _camTracker = GetComponent<CameraTrackingSwitcher>();

            var arMarkerTracker = _camTracker.MarkerTracker;
            CutoffTime = arMarkerTracker.CutoffTime;
            UseAverage = arMarkerTracker.UseAverages;
            AverageWeight = arMarkerTracker.AverageWeight;
        }

#if UNITY_EDITOR
        private void Update()
        {
            var arMarkerTracker = _camTracker.MarkerTracker;
            arMarkerTracker.CutoffTime = CutoffTime;
            arMarkerTracker.UseAverages = UseAverage;
            arMarkerTracker.AverageWeight = AverageWeight;

            if (TrackPosition)
                transform.position = arMarkerTracker.GetPosition();

            if (TrackRotation)
                transform.rotation = arMarkerTracker.GetRotation();
        }
#endif
    }
}
