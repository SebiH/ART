using UnityEngine;

namespace Assets.Modules.Calibration
{
    [RequireComponent(typeof(ChangeMonitor))]
    public class Debug_ChangeMonitor : MonoBehaviour
    {
        private ChangeMonitor _monitor;
        public float Stability = 0f;

        private void OnEnable()
        {
            _monitor = GetComponent<ChangeMonitor>();
        }

        private void Update()
        {
            _monitor.UpdateStability(transform.position, transform.rotation);
            Stability = _monitor.Stability;
        }
    }
}
