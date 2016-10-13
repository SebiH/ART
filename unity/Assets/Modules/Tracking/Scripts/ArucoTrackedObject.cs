using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class ArucoTrackedObject : MonoBehaviour
    {
        public int TrackedId;
        public bool Invert = false;

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
                if (Invert)
                {
                    Matrix4x4 marker = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
                    Matrix4x4 cam = marker.inverse;
                    transform.localPosition = cam.GetPosition();
                    transform.localRotation = cam.GetRotation();
                }
                else
                {
                    transform.localPosition = pose.Position;
                    transform.localRotation = pose.Rotation;
                }
            }
        }
    }
}
