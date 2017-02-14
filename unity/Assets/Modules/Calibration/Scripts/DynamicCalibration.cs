using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using Assets.Modules.Tracking;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class DynamicCalibration : MonoBehaviour
    {
        public ChangeMonitor OptitrackMonitor;
        public ChangeMonitor OvrMonitor;

        private Vector3 _lastOptitrackPos;
        private Quaternion _lastOptitrackRot;

        private const float OptitrackCutoffTime = 0.3f;
        private const float OptitrackChangeTolerance = 0.85f;

        private const float OvrCutoffTime = 0.3f;
        private const float OvrChangeTolerance = 0.9f;

        // angle between calibrated scene camera and surface
        private const float MinSurfaceAngle = 35f;

        private const float MarkerDetectionCutoffTime = 0.3f;
        private const float MarkerChangeCutoffTime = 0.6f;
        private const float MaxMarkerHmdDistance = 0.9f;

        void LateUpdate()
        {
            UpdateTracking();

            if (ShouldCalibrate())
            {
                PerformCalibration();
            }
        }

        private bool ShouldCalibrate()
        {
            var optitrackPose = OptitrackListener.Instance.GetPose(Globals.OptitrackHmdName);
            if (optitrackPose == null) { return false; }

            var isOptitrackPoseRecent = IsRecent(optitrackPose.DetectionTime, OptitrackCutoffTime);
            if (!isOptitrackPoseRecent) { return false; }

            var isOptitrackStable = OptitrackMonitor.Stability > OptitrackChangeTolerance;
            if (!isOptitrackStable) { return false; }


            var isHmdPoseRecent = IsRecent(VRListener.Instance.PoseUpdateTime, OvrCutoffTime);
            if (!isHmdPoseRecent) { return false; }

            var isHmdPoseStable = OvrMonitor.Stability > OvrChangeTolerance;
            if (!isHmdPoseStable) { return false; }


            // TODO: surface lookup based on current view direction? (needs initial calibration & multiple clients)
            var hasSurface = SurfaceManager.Instance.Has(Globals.DefaultSurfaceName);
            if (!hasSurface) { return false; }

            if (CalibrationParams.HasStablePosition && CalibrationParams.HasStableRotation)
            {
                var surface = SurfaceManager.Instance.Get(Globals.DefaultSurfaceName);
                var trackedCamera = SceneCameraTracker.Instance;

                // TODO: needs testing!
                //var angle = MathUtility.AngleVectorPlane(trackedCamera.transform.forward, surface.Normal);
                //var isAngleTooFlat = Mathf.Abs(angle) < MinSurfaceAngle;
                //if (isAngleTooFlat) { return false; }
            }

            return true;
        }

        private bool IsArMarkerValid(ArMarker marker)
        {
            if (!marker.HasDetectedCamera) { return false; }

            var isMarkerRecent = IsRecent(marker.CameraDetectionTime, MarkerDetectionCutoffTime);
            if (!isMarkerRecent) { return false; }

            var hasMarkerChangedRecently = IsRecent(marker.LastChangeTime, MarkerChangeCutoffTime);
            if (hasMarkerChangedRecently) { return false; }

            var optitrackPose = OptitrackListener.Instance.GetPose(Globals.OptitrackHmdName);
            var isTooFarAway = Mathf.Abs((optitrackPose.Position - marker.transform.position).magnitude) > MaxMarkerHmdDistance;
            if (isTooFarAway) { return false; }

            // TODO: angle between hmd direction & marker? (probably not necessary) - needs intersection
            // TODO: confidence, if available?

            return true;
        }

        private bool IsRecent(float time, float cutoff)
        {
            return Time.unscaledTime - time < cutoff;
        }

        private void UpdateTracking()
        {
            var optitrackPose = OptitrackListener.Instance.GetPose(Globals.OptitrackHmdName);
            if (optitrackPose != null)
            {
                var hasPositionChanged = Mathf.Abs((_lastOptitrackPos - optitrackPose.Position).magnitude) > Mathf.Epsilon;
                var hasRotationChanged = Mathf.Abs(Quaternion.Angle(_lastOptitrackRot, optitrackPose.Rotation)) > Mathf.Epsilon;

                if (hasPositionChanged || hasRotationChanged)
                {
                    _lastOptitrackPos = optitrackPose.Position;
                    _lastOptitrackRot = optitrackPose.Rotation;
                    OptitrackMonitor.UpdateStability(optitrackPose.Position, optitrackPose.Rotation);
                }
            }

            var ovrPosition = VRListener.Instance.CurrentPosition;
            var ovrRotation = VRListener.Instance.CurrentRotation;
            OvrMonitor.UpdateStability(ovrPosition, ovrRotation);
        }


        private void PerformCalibration()
        {
            var validMarkers = ArMarkers.GetAll().Where(m => IsArMarkerValid(m));

            if (validMarkers.Count() > 0)
            {
                var camPositions = validMarkers.Select(m => m.DetectedCameraPosition);
                var avgCamPosition = MathUtility.Average(camPositions);
                var optitrackPose = OptitrackListener.Instance.GetPose(Globals.OptitrackHmdName);
                Debug.Assert(optitrackPose != null, "OptitrackPose shall not be null, since we checked for that in ShouldCalibrate??");
                CalibrationParams.PositionOffset = Quaternion.Inverse(optitrackPose.Rotation) * (avgCamPosition - optitrackPose.Position);

                var camRotations = validMarkers.Select(m => m.DetectedCameraRotation);
                var avgCamRotation = MathUtility.Average(camRotations);
                var ovrRotation = VRListener.Instance.CurrentRotation;

                // MarkerRotation = Offset * Ovr
                // => Offset = MarkerRotation * inv(Ovr)
                CalibrationParams.RotationOffset = avgCamRotation * Quaternion.Inverse(ovrRotation);
            }
        }
    }
}
