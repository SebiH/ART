using Assets.Modules.Calibration;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class OptitrackCalibratedObject : MonoBehaviour
    {
        // Name in Optitrack
        public string TrackedName = "";

        void Update()
        {
            var pose = OptitrackListener.GetPose(TrackedName);
            if (pose != null)
            {
                transform.position = pose.Position + pose.Rotation * CalibrationParams.PositionOffset;
            }
        }

    }
}
