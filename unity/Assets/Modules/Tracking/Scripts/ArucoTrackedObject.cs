using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class ArucoTrackedObject : MonoBehaviour
    {
        public int TrackedId;

        void Start()
        {
            ArucoListener.Instance.NewPoseDetected += OnNewPose;
        }

        void OnDestroy()
        {
            ArucoListener.Instance.NewPoseDetected -= OnNewPose;
        }

        void OnNewPose(ArucoMarkerPose pose)
        {
            if (pose.Id == TrackedId)
            {
                transform.position = pose.Position;
                transform.rotation = pose.Rotation;
            }
        }
    }
}
