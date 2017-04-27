using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ArMarker : MonoBehaviour
    {
        public int Id = -1;
        public float LastChangeTime { get; private set; }
        public float Confidence { get; private set; }

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

        private float _size = 0.04f;
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
            ArMarkers.Add(this);
            if (ArMarkerTracker.Instance)
            {
                ArMarkerTracker.Instance.NewPoseDetected += OnArPose;
            }
            else
            {
                Debug.LogWarning("No ArMarkerTracker found!");
            }
        }

        void OnDisable()
        {
            ArMarkers.Remove(this);
            if (ArMarkerTracker.Instance)
            {
                ArMarkerTracker.Instance.NewPoseDetected -= OnArPose;
            }
        }


        void Update()
        {
            if (HasDetectedCamera && (Time.unscaledTime - CameraDetectionTime) > 0.5f)
            {
                HasDetectedCamera = false;
                Confidence = 0;
            }
        }

        private void OnArPose(MarkerPose pose)
        {
            if (pose.Id == Id)
            {
                Matrix4x4 marker = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
                Matrix4x4 cam = marker.inverse;

                HasDetectedCamera = true;
                DetectedCameraPosition = transform.position + transform.rotation * cam.GetPosition();
                DetectedCameraRotation = transform.rotation * cam.GetRotation();
                CameraDetectionTime = Time.unscaledTime;
                Confidence = (float)pose.Confidence;
            }
        }


        void OnDrawGizmos()
        {
            // Draw center
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.001f);

            // Draw marker contour
            var tl = transform.position + (transform.rotation * new Vector3(-Size / 2, Size / 2, 0));
            var tr = transform.position + (transform.rotation * new Vector3(Size / 2, Size / 2, 0));
            var bl = transform.position + (transform.rotation * new Vector3(-Size / 2, -Size / 2, 0));
            var br = transform.position + (transform.rotation * new Vector3(Size / 2, -Size / 2, 0));

            var minConfidence = ArMarkerTracker.Instance.GetMinConfidence();
            Gizmos.color = Color.Lerp(Color.red, Color.green, Mathf.Max(0, (Confidence - minConfidence) / (1.0f - minConfidence)));
            Gizmos.DrawLine(tl, bl);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);

            // Draw topleft corner for orientation
            Gizmos.DrawSphere(tl, 0.002f);

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
