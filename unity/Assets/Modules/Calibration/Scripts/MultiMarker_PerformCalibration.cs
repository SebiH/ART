using Assets.Modules.Core.Util;
using Assets.Modules.Tracking;
using System;
using System.Collections;
using System.Collections.Generic;
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
            public Vector3 Position;
            public Quaternion Rotation;
        }
        private Dictionary<int, Pose> _calibratedArucoPoses = new Dictionary<int, Pose>();



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
                    var cameraLocalPos = MatrixUtils.ExtractTranslationFromMatrix(cameraMatrix);
                    var cameraWorldPos = cMarker.Marker.transform.TransformPoint(cameraLocalPos);

                    var cameraLocalRot = MatrixUtils.ExtractRotationFromMatrix(cameraMatrix);
                    var cameraWorldRot = cMarker.Marker.transform.TransformDirection(cameraLocalRot.eulerAngles);


                    var calibratedPose = new Pose
                    {
                        Position = cameraWorldPos,
                        Rotation = Quaternion.Euler(cameraWorldRot)
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
            // TODO: rotation average!
            var avgRotOffset = Quaternion.identity;

            var samples = 0;
            var maxSamples = 10;

            while (samples < maxSamples)
            {
                var avgMarkerPos = Vector3.zero;
                var arucoPosesCount = 0;

                foreach (var pose in _calibratedArucoPoses)
                {
                    avgMarkerPos += pose.Value.Position;
                    // TODO: rotation average!
                    avgRotOffset = _ovrRot * Quaternion.Inverse(pose.Value.Rotation);
                    arucoPosesCount++;
                }

                avgMarkerPos = avgMarkerPos / arucoPosesCount;
                avgPosOffset += (avgMarkerPos - _optitrackCameraPose.Position);
                
                samples++;
                yield return new WaitForSeconds(0.1f);
            }

            avgPosOffset = avgPosOffset / samples;
            // TODO: rotation average!


            CalibrationOffset.OptitrackToCameraOffset = avgPosOffset;
            CalibrationOffset.OpenVrRotationOffset = avgRotOffset;
            CalibrationOffset.IsCalibrated = true;
            CalibrationOffset.LastCalibration = DateTime.Now;
        }
    }
}
