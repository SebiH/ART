using UnityEngine;

namespace Assets.Modules.Tracking
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
