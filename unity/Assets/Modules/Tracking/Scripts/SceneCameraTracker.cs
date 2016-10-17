using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class SceneCameraTracker : MonoBehaviour
    {
        public static SceneCameraTracker Instance;

        void OnEnable()
        {
            Instance = this;
        }
    }
}
