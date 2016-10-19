using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    // See relevant Editor script
    public class CalibrationLoader : MonoBehaviour
    {
        public string StartupFile = "";

        void OnEnable()
        {
            if (StartupFile.Length > 0)
            {
                CalibrationOffset.LoadFromFile(StartupFile);
            }
        }
    }
}
