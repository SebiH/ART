using Assets.Modules.Core;
using Assets.Modules.Core.Util;
using Assets.Modules.Tracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration
{
    public class MultiMarker_PerformCalibration : MonoBehaviour
    {
        const float SteadyPosThreshold = 0.2f;
        const float SteadyAngleThreshold = 2f;

        // will be set by script
        public bool IsReadyForCalibration = false;


        // optional camera to track camera via Aruco
        public Transform ArucoCamera;
        // will be set by script
        public bool HasSteadyArucoPose = false;

        private Vector3 _arucoPos = Vector3.zero;
        private Quaternion _arucoRot = Quaternion.identity;

        private struct Pose
        {
            public int Id;
            public Vector3 Position;
            public Quaternion Rotation;
        }
        private Dictionary<int, Pose> _calibratedArucoPoses = new Dictionary<int, Pose>();

        public enum CalibrationMethod
        {
            Standard,
            LineExtension
        }

        public CalibrationMethod CalibMethod;
        public int MaxCalibrationSamples = 10;
        public float CalibrationProgress { get; private set; }
        public bool IsCalibrating { get; private set; }


        public List<OptitrackPose.Marker> CalibrationOffsets = new List<OptitrackPose.Marker>();
        public string OptitrackCameraName = "HMD";
        // will be set by script
        public bool HasSteadyOptitrackCameraPose = false;
        private OptitrackPose _optitrackCameraPose;

        // will be set by script
        public bool HasSteadyOpenVrPose = false;
        private Quaternion _ovrRot = Quaternion.identity;


        private MultiMarker_MarkerSetup _markerSetupScript;

        void OnEnable()
        {
            IsReadyForCalibration = false;

            ArucoListener.Instance.NewPoseDetected += OnArucoPose;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Utils.Event.Listen("new_poses", OnSteamVrPose);

            _markerSetupScript = GetComponent<MultiMarker_MarkerSetup>();
        }

        void OnDisable()
        {
            ArucoListener.Instance.NewPoseDetected -= OnArucoPose;
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Utils.Event.Remove("new_poses", OnSteamVrPose);
        }


        void Update()
        {
            IsReadyForCalibration = HasSteadyOptitrackCameraPose && HasSteadyOpenVrPose && HasSteadyArucoPose;

            if (ArucoCamera != null && !CalibrationOffset.IsCalibrated)
            {
                ArucoCamera.transform.position = _arucoPos;
                ArucoCamera.transform.rotation = _arucoRot;
            }
        }

        private void OnArucoPose(ArucoMarkerPose pose)
        {
            foreach (var cMarker in _markerSetupScript.CalibratedMarkers)
            {
                if (cMarker.Id == pose.Id)
                {
                    // pose is marker's pose -> inverted we get camera pose
                    var markerMatrix = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
                    var cameraMatrix = markerMatrix.inverse;
                    var cameraLocalPos = cameraMatrix.GetPosition();
                    var cameraWorldPos = cMarker.Marker.transform.TransformPoint(cameraLocalPos);

                    var cameraLocalRot = cameraMatrix.GetRotation();
                    var cameraWorldForward = cMarker.Marker.transform.TransformDirection(cameraLocalRot * Vector3.forward);
                    var cameraWorldUp = cMarker.Marker.transform.TransformDirection(cameraLocalRot * Vector3.up);
                    var cameraWorldRot = Quaternion.LookRotation(cameraWorldForward, cameraWorldUp);

                    var calibratedPose = new Pose
                    {
                        Id = pose.Id,
                        Position = cameraWorldPos,
                        Rotation = cameraWorldRot
                    };

                    if (_calibratedArucoPoses.ContainsKey(pose.Id))
                    {
                        _calibratedArucoPoses[pose.Id] = calibratedPose;
                    }
                    else
                    {
                        _calibratedArucoPoses.Add(pose.Id, calibratedPose);
                    }

                    // TODO thresholding etc
                    HasSteadyArucoPose = true;

                    break;
                }
            }
        }



        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCameraName)
                {
                    // TODO: compare markers etc to determine steadiness
                    if (_optitrackCameraPose == null)
                    {
                        HasSteadyOptitrackCameraPose = false;
                    }
                    else
                    {
                        HasSteadyOptitrackCameraPose = true;
                    }

                    _optitrackCameraPose = pose;
                    // TODO thresholding etc
                    HasSteadyOptitrackCameraPose = true;

                    break;
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

            var prevRot = _ovrRot;
            _ovrRot = pose.rot;

            var deltaAngle = Quaternion.Angle(prevRot, _ovrRot);

            HasSteadyOpenVrPose = (Mathf.Abs(deltaAngle) < SteadyAngleThreshold);
        }



        public void Calibrate()
        {
            StartCoroutine(DoCalibration());
        }

        private IEnumerator DoCalibration()
        {
            var avgPosOffset = Vector3.zero;
            var arucoRotations = new List<Quaternion>();

            var samples = 0;
            IsCalibrating = true;

            while (samples < MaxCalibrationSamples)
            {
                foreach (var pose in _calibratedArucoPoses)
                {
                    arucoRotations.Add(pose.Value.Rotation);
                }

                if (CalibMethod == CalibrationMethod.Standard)
                {
                    var arucoPosesCount = 0;
                    var avgMarkerPos = Vector3.zero;

                    foreach (var pose in _calibratedArucoPoses)
                    {
                        avgMarkerPos += pose.Value.Position;
                        arucoPosesCount++;
                    }

                    avgMarkerPos = avgMarkerPos / arucoPosesCount;
                    avgPosOffset += (avgMarkerPos - _optitrackCameraPose.Position);
                }
                else if (CalibMethod == CalibrationMethod.LineExtension)
                {
                    var poses = _calibratedArucoPoses.Values.ToArray();
                    var closestPoints = new List<Vector3>();

                    for (int poseIndex = 0; poseIndex < poses.Length; poseIndex++)
                    {
                        var pose = poses[poseIndex];
                        var poseMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == pose.Id).Marker;
                        var poseRay = new Ray(poseMarker.transform.position, pose.Position - poseMarker.transform.position);

                        for (int intersectIndex = poseIndex + 1; intersectIndex < poses.Length; intersectIndex++)
                        {
                            var intersect = poses[intersectIndex];
                            var intersectMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == intersect.Id).Marker;
                            var intersectRay = new Ray(intersectMarker.transform.position, intersect.Position - intersectMarker.transform.position);

                            Vector3 closestPoint1, closestPoint2;
                            var success = Math3d.ClosestPointsOnTwoLines(out closestPoint1, out closestPoint2, poseRay.origin, poseRay.direction, intersectRay.origin, intersectRay.direction);

                            if (success)
                            {
                                closestPoints.Add(closestPoint1);
                                closestPoints.Add(closestPoint2);
                            }
                        }
                    }

                    if (closestPoints.Count > 0)
                    {
                        var avgIntersect = Vector3.zero;
                        foreach (var p in closestPoints)
                        {
                            avgIntersect += p;
                        }

                        avgIntersect = avgIntersect / closestPoints.Count;

                        avgPosOffset += (avgIntersect - _optitrackCameraPose.Position);
                    }
                }

                samples++;
                CalibrationProgress = samples / MaxCalibrationSamples;
                yield return new WaitForSeconds(0.1f);
            }

            avgPosOffset = avgPosOffset / samples;
            var avgRotation = QuaternionUtils.Average(arucoRotations);
            var avgRotOffset = avgRotation * Quaternion.Inverse(_ovrRot);

            CalibrationOffset.OptitrackToCameraOffset = avgPosOffset;
            CalibrationOffset.OpenVrRotationOffset = avgRotOffset;
            CalibrationOffset.IsCalibrated = true;
            CalibrationOffset.LastCalibration = DateTime.Now;

            Debug.Log("Calibration complete");
        }


        void OnDrawGizmos()
        {
            if (IsReadyForCalibration)
            {
                var avgMarkerPos = Vector3.zero;
                var arucoPosesCount = 0;
                var arucoRotations = new List<Quaternion>();

                foreach (var pose in _calibratedArucoPoses)
                {
                    avgMarkerPos += pose.Value.Position;
                    arucoRotations.Add(pose.Value.Rotation);
                    arucoPosesCount++;
                }

                var avgRotation = QuaternionUtils.Average(arucoRotations);
                avgMarkerPos = avgMarkerPos / arucoPosesCount;
                var avgPosOffset = (avgMarkerPos - _optitrackCameraPose.Position);

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_optitrackCameraPose.Position + avgPosOffset, 0.01f);
                var start = _optitrackCameraPose.Position + avgPosOffset;
                var end = start + (avgRotation * Vector3.forward);
                var up = start + (avgRotation * Vector3.up * 0.1f);
                var right = start + (avgRotation * Vector3.right * 0.1f);
                Gizmos.DrawLine(start, end);
                Gizmos.DrawLine(start, up);
                Gizmos.DrawLine(start, right);

                Gizmos.color = Color.green;
                end = start + (_ovrRot * Vector3.forward);
                up = start + (_ovrRot * Vector3.up * 0.1f);
                right = start + (_ovrRot * Vector3.right * 0.1f);
                Gizmos.DrawLine(start, end);
                Gizmos.DrawLine(start, up);
                Gizmos.DrawLine(start, right);

                foreach (var pose in _calibratedArucoPoses)
                {
                    Gizmos.color = Color.red;
                    var rot = pose.Value.Rotation;
                    start = pose.Value.Position;
                    end = start + (rot * Vector3.forward);
                    up = start + (rot * Vector3.up * 0.1f);
                    right = start + (rot * Vector3.right * 0.1f);
                    Gizmos.DrawLine(start, end);
                    Gizmos.DrawLine(start, up);
                    Gizmos.DrawLine(start, right);

                    // draw ray from marker through possible camera location
                    var marker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == pose.Key);
                    start = marker.Marker.transform.position;
                    var direction = pose.Value.Position - start;
                    end = start + 2 * direction;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(start, end);
                }

                var poses = _calibratedArucoPoses.Values.ToArray();
                Gizmos.color = Color.black;
                var closestPoints = new List<Vector3>();

                for (int poseIndex = 0; poseIndex < poses.Length; poseIndex++)
                {
                    var pose = poses[poseIndex];
                    var poseMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == pose.Id).Marker;
                    var poseRay = new Ray(poseMarker.transform.position, pose.Position - poseMarker.transform.position);

                    for (int intersectIndex = poseIndex + 1; intersectIndex < poses.Length; intersectIndex++)
                    {
                        var intersect = poses[intersectIndex];
                        var intersectMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == intersect.Id).Marker;
                        var intersectRay = new Ray(intersectMarker.transform.position, intersect.Position - intersectMarker.transform.position);

                        Vector3 closestPoint1, closestPoint2;
                        var success = Math3d.ClosestPointsOnTwoLines(out closestPoint1, out closestPoint2, poseRay.origin, poseRay.direction, intersectRay.origin, intersectRay.direction);

                        if (success)
                        {
                            Gizmos.DrawSphere(closestPoint1, 0.005f);
                            Gizmos.DrawSphere(closestPoint2, 0.005f);
                            closestPoints.Add(closestPoint1);
                            closestPoints.Add(closestPoint2);
                        }
                    }
                }

                if (closestPoints.Count > 0)
                {
                    var avgIntersect = Vector3.zero;
                    foreach (var p in closestPoints)
                    {
                        avgIntersect += p;
                    }

                    avgIntersect = avgIntersect / closestPoints.Count;
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(avgIntersect, 0.01f);
                }
            }
        }
    }
}
