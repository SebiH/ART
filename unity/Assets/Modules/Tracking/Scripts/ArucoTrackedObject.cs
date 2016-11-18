using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class ArucoTrackedObject : MonoBehaviour
    {
        public int TrackedId;
        public bool ApplyYOffset = false;
        public bool Invert = false;

        void OnEnable()
        {
            ArucoListener.Instance.NewPoseDetected += OnNewPose;
        }

        void OnDisable()
        {
            ArucoListener.Instance.NewPoseDetected -= OnNewPose;
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
                    
                    if (ApplyYOffset)
                        transform.position = camPos + camRot * pose.Position + camRot * new Vector3(0, transform.localScale.y, 0) / 2f;
                    else
                        transform.position = camPos + camRot * pose.Position;

                    transform.rotation = camRot * pose.Rotation;
                }
            }
        }
    }
}
