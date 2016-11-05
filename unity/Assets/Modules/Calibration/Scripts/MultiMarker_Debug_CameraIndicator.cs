using Assets.Modules.Tracking;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class MultiMarker_Debug_CameraIndicator : MonoBehaviour
    {
        private struct TimedPose
        {
            public DateTime DetectionTime;
            public Vector3 CameraPose;
        }

        public int MarkerId = -1;
        public Transform Visuals;
        private List<TimedPose> _savedPoses = new List<TimedPose>();
        private Color _indicationColor;

        void OnEnable()
        {
            ArucoListener.Instance.NewPoseDetected += OnArucoPose;
            var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;
            Visuals.localScale = new Vector3(markerSize, 0.001f, markerSize);
            _indicationColor = UnityEngine.Random.ColorHSV();
        }

        void OnDisable()
        {
            ArucoListener.Instance.NewPoseDetected -= OnArucoPose;
        }

        void OnArucoPose(ArucoMarkerPose pose)
        {
            if (pose.Id == MarkerId)
            {
                // pose is marker's pose -> inverted we get camera pose
                var markerMatrix = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
                var cameraMatrix = markerMatrix.inverse;
                var cameraLocalPos = cameraMatrix.GetPosition();
                var cameraWorldPos = transform.TransformPoint(cameraLocalPos);

                var camPose = new TimedPose
                {
                    DetectionTime = DateTime.Now,
                    CameraPose = cameraWorldPos
                };

                _savedPoses.Add(camPose);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = _indicationColor;

            foreach (var pose in _savedPoses)
            {
                Gizmos.DrawSphere(pose.CameraPose, 0.005f);
            }

            _savedPoses.RemoveAll((tp) => (DateTime.Now - tp.DetectionTime).TotalSeconds > 1);
        }
    }
}
