using Assets.Modules.Calibration;
using Assets.Modules.Surfaces;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface
{
    [RequireComponent(typeof(ChangeMonitor))]
    public class PropagateChangeMonitor : MonoBehaviour
    {
        public string MonitorName = "";
        private ChangeMonitor _monitor;

        private void OnEnable()
        {
            _monitor = GetComponent<ChangeMonitor>();
        }

        private void Update()
        {
            if (RemoteSurfaceConnection.Instance)
            {
                RemoteSurfaceConnection.Instance.SendCommand("Surface", "debug-cm-val-" + MonitorName,
                    string.Format("{{\"stability\": {0}, \"position\": {1}, \"rotation\": {2} }}", _monitor.Stability, _monitor.PositionStability, _monitor.RotationStability));
            }
        }
    }
}
