using UnityEngine;

namespace Assets.Modules.Calibration
{

    public class Debug_ChangeMonitor : MonoBehaviour
    {
        private ChangeMonitor _monitor;
        public float Stability = 0f;

        public string NameMonitor = "";
        public float Sensitivity = 20f;

        private void OnEnable()
        {
            _monitor = new ChangeMonitor(NameMonitor, Sensitivity);
        }

        private void Update()
        {
            _monitor._sensitivity = Sensitivity;
            _monitor.Update(transform.position, transform.rotation);
            Stability = _monitor.Stability;
        }
    }
}
