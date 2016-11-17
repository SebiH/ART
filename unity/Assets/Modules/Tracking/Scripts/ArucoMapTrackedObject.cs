using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class ArucoMapTrackedObject : MonoBehaviour
    {
        public int TrackedId;
        public bool Invert = false;

        void Start()
        {
            ArucoMapListener.Instance.NewPoseDetected += OnNewPose;
        }

        void OnDestroy()
        {
            ArucoMapListener.Instance.NewPoseDetected -= OnNewPose;
        }

        void OnNewPose(MarkerPose pose)
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
                    //transform.localPosition = pose.Position;
                    //transform.localRotation = pose.Rotation;
                    var camPos = SceneCameraTracker.Instance.transform.position;
                    var camRot = SceneCameraTracker.Instance.transform.rotation;

                    transform.position = camPos + camRot * pose.Position + camRot * new Vector3(0, transform.localScale.y, 0) / 2f;
                    transform.rotation = camRot * pose.Rotation;
                }
            }
        }
    }
}
