using Assets.Modules.Core;
using Assets.Modules.Tracking;
using Assets.Modules.Tracking.Scripts;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class DynamicCalibration : MonoBehaviour
    {
        public ChangeMonitor OptitrackMonitor;
        public ChangeMonitor OvrMonitor;

        private const float OptitrackCutoffTime = 0.2f;
        private const float OptitrackChangeTolerance = 0.8f;

        private const float OvrCutoffTime = 0.2f;
        private const float OvrChangeTolerance = 0.8f;

        void Update()
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
            if (optitrackPose == null) return false;

            var isOptitrackPoseRecent = IsRecent(optitrackPose.DetectionTime, OptitrackCutoffTime);
            if (!isOptitrackPoseRecent) return false;

            var isOptitrackStable = OptitrackMonitor.Stability > OptitrackChangeTolerance;
            if (!isOptitrackStable) return false;


            var isHmdPoseRecent = IsRecent(OpenVRListener.Instance.PoseUpdateTime, OvrCutoffTime);
            if (!isHmdPoseRecent) return false;

            var isHmdPoseStable = OvrMonitor.Stability > OvrChangeTolerance;
            if (!isHmdPoseStable) return false;

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
                OptitrackMonitor.UpdateStability(optitrackPose.Position, optitrackPose.Rotation);
            }

            var ovrPosition = OpenVRListener.Instance.CurrentPosition;
            var ovrRotation = OpenVRListener.Instance.CurrentRotation;
            OvrMonitor.UpdateStability(ovrPosition, ovrRotation);
        }


        private void PerformCalibration()
        {
#if UNITY_EDITOR

#endif

        }
    }
}
