using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    // See relevant Editor script
    public class CalibrationLoader : MonoBehaviour
    {
        public string StartupFile = "";
        public bool OverrideCalibration = false;

        public Vector3 OverrideRotation;
        public Vector3 OverridePosition;

        void OnEnable()
        {
            if (StartupFile.Length > 0)
            {
                CalibrationOffset.LoadFromFile(StartupFile);
            }
        }

        void Update()
        {
            if (OverrideCalibration)
            {
                CalibrationOffset.OptitrackToCameraOffset = OverridePosition;
                CalibrationOffset.OpenVrRotationOffset = Quaternion.Euler(OverrideRotation);
            }
            else
            {
                OverridePosition = CalibrationOffset.OptitrackToCameraOffset;
                OverrideRotation = CalibrationOffset.OpenVrRotationOffset.eulerAngles;
            }
        }
    }
}
