using Assets.Modules.Calibration;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class CalibratedVrTrackedObject : MonoBehaviour
    {
        private void Update()
        {
            transform.position = CalibrationParams.GetCalibratedPosition(VRListener.CurrentPosition, VRListener.CurrentRotation);
            transform.rotation = CalibrationParams.GetCalibratedRotation(VRListener.CurrentRotation);
        }
    }
}
