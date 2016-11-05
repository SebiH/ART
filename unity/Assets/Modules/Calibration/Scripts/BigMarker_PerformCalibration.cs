using Assets.Modules.Tracking;
using Assets.Modules.Tracking.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration
{
    public class BigMarker_PerformCalibration : MonoBehaviour
    {
        const float SteadyPosThreshold = 0.2f;
        const float SteadyAngleThreshold = 2f;
        const float ArCutoffTime = 1f / 120f;

        public float CalibrationStability { get; private set; }
        public bool InvertUpDirection = false;

        public Transform TestCamera;

        private readonly List<Vector3> _avgCalibrationPosOffsets = new List<Vector3>();
        private readonly List<Quaternion> _avgCalibrationRotOffsets = new List<Quaternion>();

        [Serializable]
        public class MarkerOffset
        {
            // set in editor
            public int ArMarkerId;
            public Corner OptitrackCorner;

            // public getters & setters so that they don't show up in unity's editor
            public bool HasArPose { get; set; }
            public float ArPoseDetectionTime { get; set; }
            public Vector3 ArMarkerPosition { get; set; }
            public Quaternion ArMarkerRotation { get; set; }
            public Vector3 ArCameraPosition { get; set; }
            public Quaternion ArCameraRotation { get; set; }
        }

        public float MarginLeft = 0.036f;
        public float MarginTop = 0.03f;
        public float OffsetY = -0.03f;

        public float MarginMarker = 0.05f;
        public int MarkersPerRow = 21;
        public int MarkersPerColumn = 12;

        public string DisplayName = "Surface";

        public MarkerOffset[] CalibrationOffsets;
        public string OptitrackCameraName = "HMD";
        private OptitrackPose _optitrackCameraPose;
        private DateTime _optitrackCameraDetectionTime;

        private Quaternion _ovrRot = Quaternion.identity;

        private Dictionary<int, MarkerOffset> ArMarkers = new Dictionary<int, MarkerOffset>();


        void OnEnable()
        {
            CalibrationOffsets = new MarkerOffset[MarkersPerRow * MarkersPerColumn];
            for (int i = 0; i < CalibrationOffsets.Length; i++) CalibrationOffsets[i] = new MarkerOffset { ArMarkerId = i };

            ArucoListener.Instance.NewPoseDetected += OnArucoPose;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Utils.Event.Listen("new_poses", OnSteamVrPose);
        }

        void OnDisable()
        {
            ArucoListener.Instance.NewPoseDetected -= OnArucoPose;
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Utils.Event.Remove("new_poses", OnSteamVrPose);
        }

        void Update()
        {
            if (FixedDisplays.Has(DisplayName))
            {
                var display = FixedDisplays.Get(DisplayName);
                var tableRotation = display.Rotation;


                var lineRenderer = GetComponent<LineRenderer>();

                if (lineRenderer != null)
                {
                    lineRenderer.SetVertexCount(5);
                    lineRenderer.SetPosition(0, display.GetCornerPosition(Corner.TopLeft));
                    lineRenderer.SetPosition(1, display.GetCornerPosition(Corner.BottomLeft));
                    lineRenderer.SetPosition(2, display.GetCornerPosition(Corner.BottomRight));
                    lineRenderer.SetPosition(3, display.GetCornerPosition(Corner.TopRight));
                    lineRenderer.SetPosition(4, display.GetCornerPosition(Corner.TopLeft));
                }


                var markers = CalibrationOffsets.Where((m) => m.HasArPose && (Time.unscaledTime - m.ArPoseDetectionTime) < ArCutoffTime);

                if (markers == null || markers.Count() != 1) return;

                var marker = markers.First();

                var markerPosWorld = GetMarkerWorldPosition(marker.ArMarkerId, tableRotation);

                var localPos = marker.ArCameraPosition;
                var worldPos = markerPosWorld + tableRotation * localPos;

                var localRot = marker.ArCameraRotation;
                var localForward = localRot * Vector3.forward;
                var localRight = localRot * Vector3.right;
                var localUp = localRot * Vector3.up;

                var worldForward = tableRotation * localForward;
                var worldRight = tableRotation * localRight;
                var worldUp = tableRotation * localUp;
                var worldRot = Quaternion.LookRotation(worldForward, worldUp);

                Debug.Log("Calibrating...");
                CalibrationParams.OptitrackToCameraOffset = Quaternion.Inverse(_optitrackCameraPose.Rotation) * (worldPos - _optitrackCameraPose.Position);
                // c = b * inv(a)
                // => b = c * a?
                // from ovrRot to worldRot
                CalibrationParams.OpenVrRotationOffset = worldRot * Quaternion.Inverse(_ovrRot);

            }
        }


        public void ResetCalibration()
        {
            _avgCalibrationPosOffsets.Clear();
            _avgCalibrationRotOffsets.Clear();
        }

        private void OnArucoPose(MarkerPose pose)
        {
            var markerOffset = CalibrationOffsets[pose.Id];

            if (markerOffset == null)
            {
                markerOffset = new MarkerOffset
                {
                    ArMarkerId = pose.Id
                };
                CalibrationOffsets[pose.Id] = markerOffset;
            }

            // pose is marker's pose -> inverted we get camera pose
            var markerMatrix = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
            var cameraMatrix = markerMatrix.inverse;
            var cameraLocalPos = cameraMatrix.GetPosition();
            var cameraLocalRot = cameraMatrix.GetRotation();

            markerOffset.HasArPose = true;
            markerOffset.ArPoseDetectionTime = Time.unscaledTime;
            markerOffset.ArMarkerPosition = pose.Position;
            markerOffset.ArMarkerRotation = pose.Rotation;
            markerOffset.ArCameraPosition = cameraMatrix.GetPosition();
            markerOffset.ArCameraRotation = cameraMatrix.GetRotation();
        }



        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCameraName)
                {
                    _optitrackCameraPose = pose;
                    _optitrackCameraDetectionTime = DateTime.Now;
                }
            }
        }


        private void OnSteamVrPose(params object[] args)
        {
            var i = (int)OpenVR.k_unTrackedDeviceIndex_Hmd;

            var poses = (TrackedDevicePose_t[])args[0];
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            _ovrRot = pose.rot;
        }

        private readonly List<Ray> _debugRays = new List<Ray>();
        private readonly List<Vector3> _debugClosestPoints = new List<Vector3>();
        private Vector3 _debugAvgPosition = Vector3.zero;
        private Quaternion _debugAvgRotation = Quaternion.identity;


        public bool IsCalibrating { get; private set; }
        public float CalibrationProgress { get; private set; }
        public void StartCalibration()
        {
            var markers = CalibrationOffsets.Where((m) => m.HasArPose && (Time.unscaledTime - m.ArPoseDetectionTime) < ArCutoffTime);
            var display = FixedDisplays.Get(DisplayName);
            var tableRotation = display.Rotation;

            if (markers == null || markers.Count() != 1) return;

            var marker = markers.First();
            //var rotations = new List<Quaternion>();
            //var positions = new List<Vector3>();

            //foreach (var marker in markers)
            //{
            var markerPosWorld = GetMarkerWorldPosition(marker.ArMarkerId, tableRotation);

            var localPos = marker.ArCameraPosition;
            var worldPos = markerPosWorld + tableRotation * localPos;

            var localRot = marker.ArCameraRotation;
            var localForward = localRot * Vector3.forward;
            var localRight = localRot * Vector3.right;
            var localUp = localRot * Vector3.up;

            var worldForward = tableRotation * localForward;
            var worldRight = tableRotation * localRight;
            var worldUp = tableRotation * localUp;
            var worldRot = Quaternion.LookRotation(worldForward, worldUp);

            Debug.Log("Calibrating...");
            CalibrationParams.OptitrackToCameraOffset = Quaternion.Inverse(_optitrackCameraPose.Rotation) * (worldPos - _optitrackCameraPose.Position);
            // c = b * inv(a)
            // => b = c * a?
            // from ovrRot to worldRot
            CalibrationParams.OpenVrRotationOffset = worldRot * Quaternion.Inverse(_ovrRot);
        }

        private class CalibPose
        {
            public Vector3 WorldMarkerPosition;
            public Vector3 WorldCameraPosition;
            public Quaternion WorldRotation;
        }


        private Vector3 GetMarkerWorldPosition(int markerIndex, Quaternion tableRotation)
        {
            var display = FixedDisplays.Get(DisplayName);

            int row = markerIndex / MarkersPerRow;
            int column = markerIndex % MarkersPerRow;
            var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;

            var markerOffsetX = MarginLeft + column * (MarginMarker + markerSize) + markerSize / 2f;
            var markerOffsetZ = -MarginTop - row * (MarginMarker + markerSize) - markerSize / 2f;

            var markerOffset = new Vector3(markerOffsetX, OffsetY, markerOffsetZ);
            return display.GetCornerPosition(Corner.TopLeft) + tableRotation * markerOffset;
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // Draw OpenVR rotation
            {
                Gizmos.color = Color.green;
                var start = SceneCameraTracker.Instance.transform.position;
                var end = start + (_ovrRot * Vector3.forward);
                var up = start + (_ovrRot * Vector3.up * 0.1f);
                var right = start + (_ovrRot * Vector3.right * 0.1f);
                Gizmos.DrawLine(start, end);
                Gizmos.DrawLine(start, up);
                Gizmos.DrawLine(start, right);
            }

            // this only works if we have optitrack coordinates for all markers
            if (FixedDisplays.Has(DisplayName))
            {
                var display = FixedDisplays.Get(DisplayName);
                var tableRotation = display.Rotation;

                // draw table's orientation in center of table
                {
                    var tableCenter = display.Position;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.up * 0.1f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.forward * 0.1f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.right * 0.1f);
                }

                for (int i = 0; i < CalibrationOffsets.Length; i++)
                {
                    if (CalibrationOffsets[i] == null)
                    {
                        CalibrationOffsets[i] = new MarkerOffset();
                    }

                    var marker = CalibrationOffsets[i];

                    // draw virtual position of calibrated markers, based on optitrack + measurements
                    Gizmos.color = Color.cyan;
                    var markerPosWorld = GetMarkerWorldPosition(i, tableRotation);
                    var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;
                    Gizmos.DrawWireSphere(markerPosWorld, 0.01f);
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, markerSize / 2), markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, markerSize / 2));
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, markerSize / 2), markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, -markerSize / 2));
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, -markerSize / 2), markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, -markerSize / 2));
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, -markerSize / 2), markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, markerSize / 2));


                    // if available, draw world position of camera based on marker
                    if (marker.HasArPose && (Time.unscaledTime - marker.ArPoseDetectionTime) < ArCutoffTime)
                    {
                        var localPos = marker.ArCameraPosition;
                        var worldPos = markerPosWorld + tableRotation * localPos;

                        var localRot = marker.ArCameraRotation;
                        var localForward = localRot * Vector3.forward;
                        var localRight = localRot * Vector3.right;
                        var localUp = localRot * Vector3.up;

                        var worldForward = tableRotation * localForward;
                        var worldRight = tableRotation * localRight;
                        var worldUp = tableRotation * localUp;
                        var worldRot = Quaternion.LookRotation(worldForward, worldUp);

                        bool isOutlier = (worldPos - _optitrackCameraPose.Position).sqrMagnitude > 0.025f;

                        Gizmos.color = Color.white;
                        Gizmos.DrawWireSphere(worldPos, 0.01f);

                        Gizmos.color = isOutlier ? Color.grey : Color.green;
                        Gizmos.DrawLine(worldPos, worldPos + worldUp * 0.1f);
                        Gizmos.color = isOutlier ? Color.grey : Color.blue;
                        Gizmos.DrawLine(worldPos, worldPos + worldForward * 0.5f);
                        Gizmos.color = isOutlier ? Color.grey : Color.red;
                        Gizmos.DrawLine(worldPos, worldPos + worldRight * 0.1f);
                    }
                }

                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(_debugAvgPosition, 0.01f);

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(_debugAvgPosition, _debugAvgPosition + _debugAvgRotation * Vector3.up * 0.1f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(_debugAvgPosition, _debugAvgPosition + _debugAvgRotation * Vector3.forward);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(_debugAvgPosition, _debugAvgPosition + _debugAvgRotation * Vector3.right * 0.1f);
                }

            }
        }
    }
}
