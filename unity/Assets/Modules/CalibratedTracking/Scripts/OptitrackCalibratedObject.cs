using Assets.Modules.Calibration;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class OptitrackCalibratedObject : MonoBehaviour
    {
        // Name in OptiTrack
        public string TrackedName = "";
        private OptitrackListener _optitrack;

        void OnEnable()
        {
            _optitrack = OptitrackListener.Instance;
            if (!_optitrack)
            {
                Debug.LogError("Missing OptitrackListener in Scene, disabling Optitrack Calibrated Object");
                gameObject.SetActive(false);
            }
        }

        void Update()
        {
            var pose = _optitrack.GetPose(TrackedName);
            if (pose != null)
            {
                transform.position = pose.Position + pose.Rotation * CalibrationParams.PositionOffset;
            }
        }

    }
}
