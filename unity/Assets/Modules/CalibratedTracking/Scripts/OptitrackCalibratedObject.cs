using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    [RequireComponent(typeof(CameraTrackingSwitcher))]
    public class OptitrackCalibratedObject : MonoBehaviour
    {
        // Name in Optitrack
        public string TrackedName = "";

        public bool TrackPosition = false;
        public bool TrackRotation = false;

        private CameraTrackingSwitcher _camTracker;

        private void OnEnable()
        {
            _camTracker = GetComponent<CameraTrackingSwitcher>();
            TrackedName = _camTracker.OptitrackTracker.TrackedName;
        }

#if UNITY_EDITOR
        void Update()
        {
            _camTracker.OptitrackTracker.TrackedName = TrackedName;

            if (TrackPosition)
                transform.position = _camTracker.OptitrackTracker.GetPosition();

            if (TrackRotation)
                transform.rotation = _camTracker.OptitrackTracker.GetRotation();
        }
#endif

    }
}
