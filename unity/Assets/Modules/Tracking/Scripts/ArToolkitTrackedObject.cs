using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArToolkitTrackedObject : MonoBehaviour
    {
        public string TrackedMarkerName;

        void Start()
        {
            ArToolkitListener.Instance.NewPoseDetected += OnNewPose;
        }

        void OnDestroy()
        {
            ArToolkitListener.Instance.NewPoseDetected -= OnNewPose;
        }

        void OnNewPose(MarkerPose pose)
        {
            // TODO.
            //if (pose.Name == TrackedMarkerName)
            {
                transform.position = pose.Position;
                transform.rotation = pose.Rotation;
            }
        }

    }
}
