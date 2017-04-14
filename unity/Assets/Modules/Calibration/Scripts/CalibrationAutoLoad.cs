using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class CalibrationAutoLoad : MonoBehaviour
    {
        private void OnEnable()
        {
            try
            {
                var calibrationJson = FileUtility.LoadFromFile(Globals.CalibrationSavefile);
                var calibration = JsonUtility.FromJson<SerializableCalibration>(calibrationJson);

                CalibrationParams.PrimeCalibration(calibration.PositionOffset, calibration.RotationOffset);
                Debug.Log("Starting with previous calibration!");
            }
            catch
            {
                Debug.LogWarning("Starting with empty calibration");
            }
        }
    }
}
