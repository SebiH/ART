using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ManualParameterAdjustment : MonoBehaviour
    {
        public bool LoadParametersOnStartup = true;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;

        private void Start()
        {
            if (LoadParametersOnStartup)
            {
                PositionOffset = CalibrationParams.PositionOffset;
                RotationOffset = CalibrationParams.RotationOffset.eulerAngles;
            }
        }

        private void LateUpdate()
        {
            if (!LoadParametersOnStartup || Time.unscaledTime > 0.5f)
            {
                CalibrationParams.PrimeCalibration(PositionOffset, Quaternion.Euler(RotationOffset));
                PositionOffset = CalibrationParams.PositionOffset;
                RotationOffset = CalibrationParams.RotationOffset.eulerAngles;
            }
        }
    }
}
