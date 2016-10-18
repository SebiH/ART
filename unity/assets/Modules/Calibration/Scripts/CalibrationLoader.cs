using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    // See relevant Editor script
    public class CalibrationLoader : MonoBehaviour
    {
        public bool LoadDefaultOnStartup = false;

        void OnEnable()
        {
            if (LoadDefaultOnStartup)
            {
                CalibrationOffset.LoadFromFile("default_calib.json");
            }
        }
    }
}
