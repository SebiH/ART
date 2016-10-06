using Assets.Modules.Core.Util;
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
            public Color Color;
        }

        public int MarkerId = -1;
        public Transform Visuals;
        private List<TimedPose> _savedPoses = new List<TimedPose>();

        void OnEnable()
        {
            ArucoListener.Instance.NewPoseDetected += OnArucoPose;
            var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;
            Visuals.localScale = new Vector3(markerSize, 0.01f, markerSize);
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
                var cameraLocalPos = MatrixUtils.ExtractTranslationFromMatrix(cameraMatrix);
                var cameraWorldPos = transform.TransformPoint(cameraLocalPos);

                var camPose = new TimedPose
                {
                    DetectionTime = DateTime.Now,
                    CameraPose = cameraWorldPos,
                    Color = UnityEngine.Random.ColorHSV()
                };

                _savedPoses.Add(camPose);
            }
        }

        void OnDrawGizmos()
        {
            foreach (var pose in _savedPoses)
            {
                Gizmos.color = pose.Color;
                Gizmos.DrawSphere(pose.CameraPose, 0.005f);
            }

            _savedPoses.RemoveAll((tp) => (DateTime.Now - tp.DetectionTime).TotalSeconds > 1);
        }
    }
}
