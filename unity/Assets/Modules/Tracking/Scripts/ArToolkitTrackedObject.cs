using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArToolkitTrackedObject : MonoBehaviour
    {
        public string TrackedMarkerName;

        void OnEnable()
        {
            ArToolkitListener.Instance.NewPoseDetected += OnNewPose;
        }

        void OnDisable()
        {
            ArToolkitListener.Instance.NewPoseDetected -= OnNewPose;
        }

        void OnNewPose(MarkerPose pose)
        {
            if (pose.Name == TrackedMarkerName)
            {
                transform.position = pose.Position;
                transform.rotation = pose.Rotation;
            }
        }

    }
}
