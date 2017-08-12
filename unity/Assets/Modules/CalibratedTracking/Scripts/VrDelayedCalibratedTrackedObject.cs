using UnityEngine;

namespace Assets.Modules.CalibratedTracking.Scripts
{
    [RequireComponent(typeof(CameraTrackingSwitcher))]
    public class VrDelayedCalibratedTrackedObject : MonoBehaviour
    {
        public bool TrackPosition = false;
        public bool TrackRotation = false;

        [Range(-0.1f, 0.1f)]
        public float TrackingDelay = 0f;

        private CameraTrackingSwitcher _camTracker;

        private void OnEnable()
        {
            _camTracker = GetComponent<CameraTrackingSwitcher>();
            _camTracker.VrTracker.TrackingDelay = TrackingDelay;
        }

#if UNITY_EDITOR
        private void Update()
        {
            _camTracker.VrTracker.TrackingDelay = TrackingDelay;
            TrackingDelay = _camTracker.VrTracker.TrackingDelay;

            if (TrackPosition)
                transform.position = _camTracker.VrTracker.GetPosition();
            if (TrackRotation)
                transform.rotation = _camTracker.VrTracker.GetRotation();
        }
#endif

    }
}
