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
                // TODO: should be HMDGap from OvrVision..?
                var offset = new Vector3(-0.032f, 0.0f, 0.0f);
                transform.position = pose.Position + offset;
                transform.rotation = pose.Rotation;
            }
        }
    }
}
