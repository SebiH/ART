using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    [RequireComponent(typeof(CameraTrackingSwitcher))]
    public class MarkerCameraTracker : MonoBehaviour
    {
        // in seconds
        public float CutoffTime = 0.6f;
        public bool UseAnimationSmoothing = true;
        [Range(0.01f, 0.5f)]
        public float AnimationSmoothing = 0.1f;
        public bool IgnoreSmallChanges = true;

        public bool TrackPosition = false;
        public bool TrackRotation = false;

        private CameraTrackingSwitcher _camTracker;

        private void OnEnable()
        {
            _camTracker = GetComponent<CameraTrackingSwitcher>();

            var arMarkerTracker = _camTracker.MarkerTracker;
            CutoffTime = arMarkerTracker.CutoffTime;
            UseAnimationSmoothing = arMarkerTracker.UseAnimationSmoothing;
            AnimationSmoothing = arMarkerTracker.AnimationSmoothing;
            IgnoreSmallChanges = arMarkerTracker.IgnoreSmallChanges;
        }

#if UNITY_EDITOR
        private void Update()
        {
            var arMarkerTracker = _camTracker.MarkerTracker;
            arMarkerTracker.CutoffTime = CutoffTime;
            arMarkerTracker.UseAnimationSmoothing = UseAnimationSmoothing;
            arMarkerTracker.AnimationSmoothing = AnimationSmoothing;
            arMarkerTracker.IgnoreSmallChanges = IgnoreSmallChanges;

            if (TrackPosition)
                transform.position = arMarkerTracker.GetPosition();

            if (TrackRotation)
                transform.rotation = arMarkerTracker.GetRotation();
        }

        private void OnDrawGizmos()
        {
            if (_camTracker)
            {
                _camTracker.MarkerTracker.DrawGizmos();
            }
        }
#endif
    }
}
