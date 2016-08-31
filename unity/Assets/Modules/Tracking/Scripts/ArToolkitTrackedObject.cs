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

        void OnNewPose(string detectedMarkerName, Vector3 position, Quaternion rotation)
        {
            // TODO.
            //if (detectedMarkerName == TrackedMarkerName)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }

    }
}
