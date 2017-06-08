using UnityEngine;

namespace Assets.Modules.Core
{
    public class SetResolution : MonoBehaviour
    {
        private void Start()
        {
            Screen.SetResolution(1920 * 2, 1080 * 2, true, 60);
        }
    }
}
