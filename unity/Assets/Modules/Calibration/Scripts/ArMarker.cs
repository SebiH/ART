using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ArMarker : MonoBehaviour
    {
        public int Id { get; set; }
        public float LastChangeTime { get; private set; }

        private Vector2 _position = Vector2.zero;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                transform.localPosition = new Vector3(value.x, 0, value.y);
                LastChangeTime = Time.unscaledTime;
                HasDetectedCamera = false;
            }
        }

        private float _size = 0.25f;
        public float Size
        {
            get { return _size; }
            set
            {
                _size = value;
                LastChangeTime = Time.unscaledTime;
            }
        }

        public bool HasDetectedCamera { get; private set; }
        public Vector3 DetectedCameraPosition { get; private set; }
        public Quaternion DetectedCameraRotation { get; private set; }
        public float CameraDetectionTime { get; private set; }

        void OnEnable()
        {
            Register();
            ArMarkers.Add(this);
            if (ArucoListener.Instance)
            {
                ArucoListener.Instance.NewPoseDetected += OnArPose;
            }
        }

        void OnDisable()
        {
            Deregister();
            ArMarkers.Remove(this);
            if (ArucoListener.Instance)
            {
                ArucoListener.Instance.NewPoseDetected -= OnArPose;
            }
        }

        private void Register()
        {
            // TODO
        }

        private void Deregister()
        {
            // TODO
        }


        void Update()
        {
            if (HasDetectedCamera && (Time.unscaledTime - CameraDetectionTime) > 0.5f)
            {
                HasDetectedCamera = false;
            }
        }

        private void OnArPose(MarkerPose pose)
        {
            Matrix4x4 marker = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
            Matrix4x4 cam = marker.inverse;

            HasDetectedCamera = true;
            DetectedCameraPosition = transform.position + cam.GetPosition();
            DetectedCameraRotation = transform.rotation * cam.GetRotation();
            CameraDetectionTime = Time.unscaledTime;
        }


        void OnDrawGizmos()
        {
            // Draw center
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.01f);

            // Draw marker contour
            var tl = transform.position - (transform.rotation * new Vector3(-Size / 2, 0, -Size / 2));
            var tr = transform.position - (transform.rotation * new Vector3(Size / 2, 0, -Size / 2));
            var bl = transform.position - (transform.rotation * new Vector3(-Size / 2, 0, Size / 2));
            var br = transform.position - (transform.rotation * new Vector3(Size / 2, 0, Size / 2));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(tl, bl);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);

            if (HasDetectedCamera)
            {
                // draw detected camera pose
                Gizmos.DrawWireSphere(DetectedCameraPosition, 0.01f);

                // draw rotation of camera
                Gizmos.color = Color.blue;
                var camForward = DetectedCameraRotation * Vector3.forward;
                Gizmos.DrawLine(DetectedCameraPosition, DetectedCameraPosition + camForward * 0.1f);

                Gizmos.color = Color.green;
                var camUp = DetectedCameraRotation * Vector3.up;
                Gizmos.DrawLine(DetectedCameraPosition, DetectedCameraPosition + camUp * 0.01f);

                Gizmos.color = Color.red;
                var camRight = DetectedCameraRotation * Vector3.right;
                Gizmos.DrawLine(DetectedCameraPosition, DetectedCameraPosition + camRight * 0.01f);

                // draw connection to marker for clarity
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(DetectedCameraPosition, transform.position);
            }
        }
    }
}
