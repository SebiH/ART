using Assets.Modules.Core;
using Assets.Modules.Core.Util;
using Assets.Modules.Tracking;
using Assets.Modules.Tracking.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration
{
    public class DisplayMarker_PerformCalibration : MonoBehaviour
    {
        const float SteadyPosThreshold = 0.2f;
        const float SteadyAngleThreshold = 2f;
        const float ArCutoffTime = 0.1f;

        public float CalibrationStability { get; private set; }
        public bool InvertUpDirection = false;

        public Transform TestCamera;

        public enum CalibrationMethod
        {
            Standard,
            LineExtension
        }

        public CalibrationMethod CalibMethod;

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

        public float MarginLeft = 0.047f;
        public float MarginTop = 0.046f;
        public float OffsetY = -0.03f;

        public float MarginMarker = 0.036f;
        public int MarkersPerRow = 21;
        public int MarkersPerColumn = 12;

        public MarkerOffset[] CalibrationOffsets;
        public string OptitrackCameraName = "HMD";
        private OptitrackPose _optitrackCameraPose;
        private DateTime _optitrackCameraDetectionTime;

        public DisplayMarker_SetupDisplay DisplaySetupScript;

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
            if (DisplaySetupScript.CalibratedCorners.Count >= 4)
            {
                var lineRenderer = GetComponent<LineRenderer>();

                if (lineRenderer != null)
                {
                    var tableRotation = CalculateTableRotation();
                    lineRenderer.SetVertexCount(5);
                    var topleft = DisplaySetupScript.CalibratedCorners.First((c) => c.Corner == Corner.TopLeft).Position + tableRotation * new Vector3(0, OffsetY, 0);
                    var bottomleft = DisplaySetupScript.CalibratedCorners.First((c) => c.Corner == Corner.BottomLeft).Position + tableRotation * new Vector3(0, OffsetY, 0);
                    var bottomright = DisplaySetupScript.CalibratedCorners.First((c) => c.Corner == Corner.BottomRight).Position + tableRotation * new Vector3(0, OffsetY, 0);
                    var topright = DisplaySetupScript.CalibratedCorners.First((c) => c.Corner == Corner.TopRight).Position + tableRotation * new Vector3(0, OffsetY, 0);

                    lineRenderer.SetPosition(0, topleft);
                    lineRenderer.SetPosition(1, bottomleft);
                    lineRenderer.SetPosition(2, bottomright);
                    lineRenderer.SetPosition(3, topright);
                    lineRenderer.SetPosition(4, topleft);
                }
            }


            if (TestCamera != null && CalibrationOffsets != null)
            {
                var markers = CalibrationOffsets.Where((m) => m.HasArPose && (Time.unscaledTime - m.ArPoseDetectionTime) < ArCutoffTime && m.ArMarkerId == 212);
                var tableRotation = CalculateTableRotation();

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

                //    rotations.Add(worldRot);
                //    positions.Add(worldPos);
                //}


                //TestCamera.transform.position = QuaternionUtils.AverageV(positions);
                //TestCamera.transform.rotation = QuaternionUtils.Average(rotations);
                TestCamera.transform.position = worldPos;
                TestCamera.transform.rotation = worldRot;


                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Calibrating...");
                    CalibrationOffset.IsCalibrated = true;
                    CalibrationOffset.LastCalibration = DateTime.Now;
                    CalibrationOffset.OptitrackToCameraOffset = Quaternion.Inverse(_optitrackCameraPose.Rotation) * (_optitrackCameraPose.Position - worldPos);
                    // c = b * inv(a)
                    // => b = c * a?
                    // from ovrRot to worldRot
                    CalibrationOffset.OpenVrRotationOffset = worldRot * Quaternion.Inverse(_ovrRot);
                }
            }
        }


        public void ResetCalibration()
        {
            _avgCalibrationPosOffsets.Clear();
            _avgCalibrationRotOffsets.Clear();
        }

        private void OnArucoPose(ArucoMarkerPose pose)
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
            if (!IsCalibrating)
            {
                StartCoroutine(UpdateCalibration());
            }
        }

        private class CalibPose
        {
            public Vector3 WorldMarkerPosition;
            public Vector3 WorldCameraPosition;
            public Quaternion WorldRotation;
        }

        private IEnumerator UpdateCalibration()
        {
            var avgPos = new List<Vector3>();
            var avgRot = new List<Quaternion>();

            IsCalibrating = true;

            const int maxSamples = 100;
            for (int sampleCount = 0; sampleCount < maxSamples; sampleCount++)
            {
                CalibrationProgress = sampleCount / (float)maxSamples;
                yield return new WaitForSecondsRealtime(0.002f);

                // this only works if we have optitrack coordinates for all markers
                if (DisplaySetupScript.CalibratedCorners.Count >= 4)
                {
                    var tableRotation = CalculateTableRotation();

                    var calibPoses = new List<CalibPose>();
                    var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;

                    for (int i = 0; i < CalibrationOffsets.Length; i++)
                    {
                        if (CalibrationOffsets[i] == null)
                        {
                            continue;
                        }

                        var marker = CalibrationOffsets[i];
                        var markerPosWorld = GetMarkerWorldPosition(i, tableRotation);
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

                            calibPoses.Add(new CalibPose
                            {
                                WorldCameraPosition = worldPos,
                                WorldMarkerPosition = markerPosWorld,
                                WorldRotation = worldRot
                            });
                        }
                    }

                    if (calibPoses.Count == 0)
                    {
                        continue;
                    }

                    // camera *must* be near optitrack camera position
                    var poses_noOutlier = calibPoses.Where((p) => (p.WorldCameraPosition - _optitrackCameraPose.Position).sqrMagnitude <= 0.025f);
                    Debug.Log("Found " + (calibPoses.Count - poses_noOutlier.Count()) + " Outliers during calibration");

                    var avgPosition = Vector3.zero;
                    var rots = new List<Quaternion>();

                    foreach (var pose in poses_noOutlier)
                    {
                        avgPosition += pose.WorldCameraPosition;
                        // marker/table rotation??
                        rots.Add(pose.WorldRotation);
                    }

                    // need a few points to minimize errors
                    if (rots.Count > 4)
                    {
                        if (CalibMethod == CalibrationMethod.Standard)
                        {
                            avgPosition = avgPosition / rots.Count;
                            avgPos.Add(Quaternion.Inverse(_optitrackCameraPose.Rotation) * (avgPosition - _optitrackCameraPose.Position));
                            _debugAvgPosition = avgPosition;
                        }
                        else if (CalibMethod == CalibrationMethod.LineExtension)
                        {
                            var closestPoints = new List<Vector3>();
                            var poses = poses_noOutlier.ToArray();

                            _debugClosestPoints.Clear();
                            _debugRays.Clear();

                            for (int poseIndex = 0; poseIndex < poses.Length; poseIndex++)
                            {
                                var pose = poses[poseIndex];

                                var poseRay = new Ray(pose.WorldMarkerPosition, pose.WorldCameraPosition - pose.WorldMarkerPosition);
                                _debugRays.Add(poseRay);

                                for (int intersectIndex = poseIndex + 1; intersectIndex < poses.Length; intersectIndex++)
                                {
                                    var intersect = poses[intersectIndex];
                                    var intersectRay = new Ray(intersect.WorldMarkerPosition, intersect.WorldCameraPosition - intersect.WorldMarkerPosition);

                                    Vector3 closestPoint1, closestPoint2;
                                    var success = Math3d.ClosestPointsOnTwoLines(out closestPoint1, out closestPoint2, poseRay.origin, poseRay.direction, intersectRay.origin, intersectRay.direction);

                                    if (success)
                                    {
                                        closestPoints.Add(closestPoint1);
                                        closestPoints.Add(closestPoint2);

                                        _debugClosestPoints.Add(closestPoint1);
                                        _debugClosestPoints.Add(closestPoint2);
                                    }
                                }
                            }

                            if (closestPoints.Count > 0)
                            {
                                avgPosition = Vector3.zero;
                                foreach (var p in closestPoints)
                                {
                                    avgPosition += p;
                                }

                                avgPosition = avgPosition / closestPoints.Count;
                                avgPos.Add(Quaternion.Inverse(_optitrackCameraPose.Rotation) * (avgPosition - _optitrackCameraPose.Position));
                                _debugAvgPosition = avgPosition;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Unknown calibration method " + CalibMethod.ToString());
                        }

                        var avgRotation = MathUtils.AverageQuaternion(rots);
                        avgRot.Add(avgRotation * Quaternion.Inverse(_ovrRot));
                        _debugAvgRotation = avgRotation;
                    }
                }
            }

            var avgPosOffset = Vector3.zero;
            foreach (var pos in avgPos) { avgPosOffset += pos; }

            _avgCalibrationPosOffsets.Add(avgPosOffset / avgPos.Count);
            _avgCalibrationRotOffsets.Add(MathUtils.AverageQuaternion(avgRot));

            var totalAvgPosOffset = Vector3.zero;
            foreach (var offset in _avgCalibrationPosOffsets) { totalAvgPosOffset += offset; }

            CalibrationOffset.OptitrackToCameraOffset = totalAvgPosOffset / _avgCalibrationPosOffsets.Count;
            CalibrationOffset.OpenVrRotationOffset = MathUtils.AverageQuaternion(_avgCalibrationRotOffsets);

            CalibrationOffset.IsCalibrated = true;
            CalibrationOffset.LastCalibration = DateTime.Now;

            IsCalibrating = false;
        }


        private Vector3 GetOptitrackMarkerPosition(MarkerOffset marker)
        {
            var corner = DisplaySetupScript.CalibratedCorners.FirstOrDefault((c) => c.Corner == marker.OptitrackCorner);
            if (corner == null)
            {
                Debug.LogWarning("Could not find matching marker for " + marker.OptitrackCorner.ToString());
                return Vector3.zero;
            }
            return corner.Position;
        }

        private Quaternion CalculateTableRotation()
        {
            var markerBottomLeft = DisplaySetupScript.CalibratedCorners.First((m) => m.Corner == Corner.BottomLeft);
            var markerTopLeft = DisplaySetupScript.CalibratedCorners.First((m) => m.Corner == Corner.TopLeft);
            var markerBottomRight = DisplaySetupScript.CalibratedCorners.First((m) => m.Corner == Corner.BottomRight);

            var forward = Vector3.Normalize(markerTopLeft.Position - markerBottomLeft.Position);
            var right = Vector3.Normalize(markerBottomRight.Position - markerBottomLeft.Position);
            var up = Vector3.Cross(forward, right);
            // Cross product doesn't always point in the correct direction
            if (InvertUpDirection) { up = -up; }

            return Quaternion.LookRotation(forward, up);
        }

        private Vector3 GetMarkerWorldPosition(int markerIndex, Quaternion tableRotation)
        {
            var otmTopLeft = DisplaySetupScript.CalibratedCorners.FirstOrDefault((c) => c.Corner == Corner.TopLeft);
            if (otmTopLeft == null) { return Vector3.zero; }

            int row = markerIndex / MarkersPerRow;
            int column = markerIndex % MarkersPerRow;
            var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;

            var markerOffsetX = MarginLeft + column * (MarginMarker + markerSize) + markerSize / 2f;
            var markerOffsetZ = -MarginTop - row * (MarginMarker + markerSize) - markerSize / 2f;

            var markerOffset = new Vector3(markerOffsetX, OffsetY, markerOffsetZ);
            return otmTopLeft.Position + tableRotation * markerOffset;
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
            if (DisplaySetupScript.CalibratedCorners.Count >= 4)
            {
                var tableRotation = CalculateTableRotation();

                // draw table's orientation in center of table
                {
                    Vector3 tableCenter = Vector3.zero;
                    foreach (var marker in DisplaySetupScript.CalibratedCorners)
                    {
                        tableCenter += marker.Position;
                    }
                    tableCenter /= DisplaySetupScript.CalibratedCorners.Count;

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.up * 0.1f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.forward * 0.1f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.right * 0.1f);
                }

                foreach (var corner in DisplaySetupScript.CalibratedCorners)
                {
                    var nextMarkerCorner = ((Corner)(((int)corner.Corner + 1) % Enum.GetNames(typeof(Corner)).Length));
                    var nextMarker = DisplaySetupScript.CalibratedCorners.Find((m) => m.Corner == nextMarkerCorner);

                    //draw lines around optitrack plane
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(corner.Position, nextMarker.Position);

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

                if (CalibMethod == CalibrationMethod.LineExtension)
                {
                    Gizmos.color = Color.yellow;
                    foreach (var ray in _debugRays)
                    {
                        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction);
                    }

                    Gizmos.color = Color.black;
                    foreach (var pos in _debugClosestPoints)
                    {
                        Gizmos.DrawSphere(pos, 0.001f);
                    }
                }
            }
        }
    }
}
