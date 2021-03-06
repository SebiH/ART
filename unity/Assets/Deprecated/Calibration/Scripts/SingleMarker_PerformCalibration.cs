//#define USE_ARTOOLKIT

using Assets.Modules.Calibration;
using Assets.Modules.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;


namespace Assets.Modules.Calibration_Deprecated
{
    public class SingleMarker_PerformCalibration : MonoBehaviour
    {
        const float SteadyPosThreshold = 0.2f;
        const float SteadyAngleThreshold = 2f;

        // will be set by script
        public bool IsReadyForCalibration = false;

#if (USE_ARTOOLKIT)
        // optional camera to track camera via ArToolkit
        public Transform ArtkCamera;
        public string ArtkCalibrationName = "kanji";
        // will be set by script
        public bool HasSteadyArtkPose = false;

        private Vector3 _artkPos = Vector3.zero;
        private Quaternion _artkRot  = Quaternion.identity;
#else
        // optional camera to track camera via Aruco
        public Transform ArucoCamera;
        public int ArucoCalibrationId = 56;
        // will be set by script
        public bool HasSteadyArucoPose = false;

        private Vector3 _arucoPos = Vector3.zero;
        private Quaternion _arucoRot = Quaternion.identity;
#endif

        public List<OptitrackPose.Marker> CalibrationParamss = new List<OptitrackPose.Marker>();
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
#if (USE_ARTOOLKIT)
            ArToolkitListener.Instance.NewPoseDetected += OnArtkPose;
#else
            ArMarkerTracker.Instance.NewPoseDetected += OnArucoPose;
#endif
            OptitrackListener.PosesReceived += OnOptitrackPose;
            SteamVR_Events.NewPoses.Listen(OnSteamVrPose);
        }

        void OnDisable()
        {
#if (USE_ARTOOLKIT)
            ArToolkitListener.Instance.NewPoseDetected -= OnArtkPose;
#else
            ArMarkerTracker.Instance.NewPoseDetected -= OnArucoPose;
#endif
            OptitrackListener.PosesReceived -= OnOptitrackPose;
            SteamVR_Events.NewPoses.Remove(OnSteamVrPose);
        }

        void Update()
        {
#if (USE_ARTOOLKIT)
            IsReadyForCalibration = HasSteadyOptitrackCameraPose && HasSteadyOptitrackCalibrationPose && HasSteadyOpenVrPose && HasSteadyArtkPose;

            if (ArtkCamera != null)
            {
                ArtkCamera.transform.position = _artkPos;
                ArtkCamera.transform.rotation = _artkRot;
            }
#else
            IsReadyForCalibration = HasSteadyOptitrackCameraPose && HasSteadyOptitrackCalibrationPose && HasSteadyOpenVrPose && HasSteadyArucoPose;

            if (ArucoCamera != null)
            {
                ArucoCamera.transform.localPosition = _arucoPos;
                ArucoCamera.transform.localRotation = _arucoRot;
            }
#endif
        }


#if (USE_ARTOOLKIT)
        private void OnArtkPose(MarkerPose pose)
        {
            if (pose.Name == ArtkCalibrationName)
            {
                // we're interested in camera's position relative to marker, not markerposition
                // -> we can get camera position by inverting marker transformation matrix

                var invertedPose = pose.Inverse();

                var prevPos = _artkPos;
                var prevRot = _artkRot;

                _artkPos = invertedPose.Position;
                _artkRot = invertedPose.Rotation;

                var hasSteadyPos = (_artkPos - prevPos).sqrMagnitude < SteadyPosThreshold;
                var hasSteadyRot = Quaternion.Angle(prevRot, _artkRot) < SteadyAngleThreshold;
                HasSteadyArtkPose = hasSteadyPos && hasSteadyRot;
            }
        }
#else

        private void OnArucoPose(MarkerPose pose)
        {
            if (pose.Id == ArucoCalibrationId)
            {
                // we're interested in camera's position relative to marker, not markerposition
                // -> we can get camera position by inverting marker transformation matrix
                var transformMatrix = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
                var invMatrix = transformMatrix.inverse;

                var prevPos = _arucoPos;
                var prevRot = _arucoRot;

                _arucoPos = invMatrix.GetPosition();
                _arucoRot = invMatrix.GetRotation();

                var hasSteadyPos = (_arucoPos - prevPos).sqrMagnitude < SteadyPosThreshold;
                var hasSteadyRot = Quaternion.Angle(prevRot, _arucoRot) < SteadyAngleThreshold;
                HasSteadyArucoPose = hasSteadyPos && hasSteadyRot;
            }
        }
#endif

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

        private void OnSteamVrPose(TrackedDevicePose_t[] poses)
        {
            var i = (int)OpenVR.k_unTrackedDeviceIndex_Hmd;

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
                Debug.LogWarning("Cannot perform calibration, not yet ready");
                return;
            }

            /*
             *  Position
             */

            // offset optitrack -> marker
            var markerPosInRoom = Vector3.zero;

            if (CalibrationParamss.Count != 0)
            {
                // build average over all given offsets
                foreach (var calibOffset in CalibrationParamss)
                {
                    var matchingMarker = _optitrackCalibrationPose.Markers.First(m => m.Id == calibOffset.Id);
                    markerPosInRoom += (matchingMarker.Position + _optitrackCalibrationPose.Rotation * calibOffset.Position);
                }

                markerPosInRoom = markerPosInRoom / CalibrationParamss.Count;
            }

#if (USE_ARTOOLKIT)
            var artkCameraPosInRoom = _artkPos + markerPosInRoom; // TODO: could be '-' instead of '+' ?
#else
            var artkCameraPosInRoom = _arucoPos + markerPosInRoom; // TODO: could be '-' instead of '+' ?
#endif

            CalibrationParams.PositionOffset = (artkCameraPosInRoom - _optitrackCameraPose.Position);


            /*
             *  Rotation
             */

            // adjust marker to optitrack's coordinate system
            var markerRotationInRoom = _optitrackCalibrationPose.Rotation;

            // relation 'marker -> camera' is now based on room coordinates,
            // due to anchoring the marker in optitrack's coordinate system
            // - the camera's rotation + calibrationobjects rotation therefore 
            // should be the correct rotation in the room
#if (USE_ARTOOLKIT)
            var cameraRotationInRoom =  markerRotationInRoom * _artkRot;
#else
            var cameraRotationInRoom = markerRotationInRoom * _arucoRot;
#endif

            // this should be the direction in which the OpenVR headset *should*
            // be looking - anything else we'll save as offset
            CalibrationParams.RotationOffset  = _ovrRot * Quaternion.Inverse(cameraRotationInRoom);
        }
    }
}
