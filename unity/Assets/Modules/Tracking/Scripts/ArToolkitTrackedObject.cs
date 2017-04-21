using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArToolkitTrackedObject : MonoBehaviour
    {
        public int TrackedMarkerId;

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
            if (pose.Id == TrackedMarkerId)
            {
                transform.position = pose.Position;
                transform.rotation = pose.Rotation;
            }
        }

    }
}
