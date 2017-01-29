using UnityEngine;

namespace Assets.Modules.Calibration
{

    public class Debug_ChangeMonitor : MonoBehaviour
    {
        private readonly ChangeMonitor _monitor = new ChangeMonitor();
        public float Stability = 0f;

        private void Update()
        {
            _monitor.Update(transform.position, transform.rotation);
            Stability = _monitor.StabilityLevel;
        }
    }
}
