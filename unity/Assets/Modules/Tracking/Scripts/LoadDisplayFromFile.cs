using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class LoadDisplayFromFile : MonoBehaviour
    {
        public string StartupFilename = "default_displays.json";

        void OnEnable()
        {
            FixedDisplays.LoadFromFile(StartupFilename);
        }
    }
}
