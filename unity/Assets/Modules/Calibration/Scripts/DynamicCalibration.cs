using Assets.Modules.Core;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class DynamicCalibration : MonoBehaviour
    {
        private const float OptitrackCutoffTime = 0.2f;

        void Update()
        {
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

            return true;
        }

        private bool IsRecent(float time, float cutoff)
        {
            return Time.unscaledTime - time < cutoff;
        }


        private void PerformCalibration()
        {
#if UNITY_EDITOR

#endif

        }
    }
}
