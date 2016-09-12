using Assets.Modules.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Tracking
{
    public class PerformCalibration : MonoBehaviour
    {
        const float SteadyPosThreshold = 0.2f;
        const float SteadyAngleThreshold = 2f;

        // will be set by script
        public bool IsReadyForCalibration = false;

        // optional camera to track camera via ArToolkit
        public Camera ArtkCamera;
        public string ArtkCalibrationName = "kanji";
        // will be set by script
        public bool HasSteadyArtkPose = false;

        private Vector3 _artkPos = Vector3.zero;
        private Quaternion _artkRot  = Quaternion.identity;


        public List<OptitrackPose.Marker> CalibrationOffsets = new List<OptitrackPose.Marker>();
        public string OptitrackCalibrationName = "CalibrationHelper";
        public string OptitrackCameraName = "HMD";
        // will be set by script
        public bool HasSteadyOptitrackCalibrationPose = false;
        // will be set by script
        public bool HasSteadyOptitrackCameraPose = false;

        private OptitrackPose _optitrackCalibrationPose;
        private OptitrackPose _optitrackCameraPose;

        // will be set by script
        public bool HasSteadyOpenVrPose = false;
        private Quaternion _ovrRot = Quaternion.identity;


        void OnEnable()
        {
            IsReadyForCalibration = false;
            ArToolkitListener.Instance.NewPoseDetected += OnArtkPose;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Utils.Event.Listen("new_poses", OnSteamVrPose);
        }

        void OnDisable()
        {
            ArToolkitListener.Instance.NewPoseDetected -= OnArtkPose;
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Utils.Event.Remove("new_poses", OnSteamVrPose);
        }

        void Update()
        {
            IsReadyForCalibration = HasSteadyOptitrackCameraPose && HasSteadyOptitrackCalibrationPose && HasSteadyOpenVrPose && HasSteadyArtkPose;

            if (ArtkCamera != null)
            {
                ArtkCamera.transform.localPosition = _artkPos;
                ArtkCamera.transform.localRotation = _artkRot;
            }
        }


        private void OnArtkPose(MarkerPose pose)
        {
            if (pose.Name == ArtkCalibrationName)
            {
                // we're interested in camera's position relative to marker, not markerposition
                // -> we can get camera position by inverting marker transformation matrix
                var invPoseMatrix = pose.PoseMatrix;

                var prevPos = _artkPos;
                var prevRot = _artkRot;

                _artkPos = MatrixUtils.ExtractTranslationFromMatrix(invPoseMatrix);
                _artkRot = MatrixUtils.ExtractRotationFromMatrix(invPoseMatrix);

                var hasSteadyPos = (_artkPos - prevPos).sqrMagnitude < SteadyPosThreshold;
                var hasSteadyRot = Quaternion.Angle(prevRot, _artkRot) < SteadyAngleThreshold;
                HasSteadyArtkPose = hasSteadyPos && hasSteadyRot;
            }
        }

        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCalibrationName)
                {
                    SaveOptitrackPose(pose, ref _optitrackCalibrationPose, ref HasSteadyOptitrackCalibrationPose);
                }

                if (pose.RigidbodyName == OptitrackCameraName)
                {
                    SaveOptitrackPose(pose, ref _optitrackCameraPose, ref HasSteadyOptitrackCameraPose);
                }
            }
        }

        private void SaveOptitrackPose(OptitrackPose newPose, ref OptitrackPose memberPose, ref bool steadyIndicator)
        {
            if (memberPose == null)
            {
                steadyIndicator = false;
            }
            else
            {
                //var prevPose = memberPose;
                // TODO: compare markers etc...
                steadyIndicator = true;
            }

            memberPose = newPose;
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
            if (!IsReadyForCalibration)
            {
                Debug.LogWarning("Cannot performa calibration, not yet ready");
                return;
            }

            CalibrationOffset.IsCalibrated = true;
            CalibrationOffset.LastCalibration = DateTime.Now;

            /*
             *  Position
             */

            // offset optitrack -> marker
            var markerPosInRoom = Vector3.zero;

            if (CalibrationOffsets.Count != 0)
            {
                // build average over all given offsets
                foreach (var calibOffset in CalibrationOffsets)
                {
                    var matchingMarker = _optitrackCalibrationPose.Markers.First(m => m.Id == calibOffset.Id);
                    markerPosInRoom += (matchingMarker.Position + calibOffset.Position);
                }

                markerPosInRoom = markerPosInRoom / CalibrationOffsets.Count;
            }

            var artkCameraPosInRoom = _artkPos + markerPosInRoom; // TODO: could be '-' instead of '+' ?

            CalibrationOffset.OptitrackToCameraOffset = (artkCameraPosInRoom - _optitrackCameraPose.Position);



            /*
             *  Rotation
             */

            var markerRotationInRoom = _optitrackCalibrationPose.Rotation;
            // rotate artk camera accordingly
            // save offset between camera rot and ovrRot

            //CalibrationOffset.OpenVrRotationOffset = ;
        }
    }
}
