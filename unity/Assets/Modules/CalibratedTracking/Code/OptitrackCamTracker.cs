using Assets.Modules.Calibration;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public class OptitrackCamTracker : CamTracker
    {
        public string TrackedName = "HMD";

        private Vector3 _position;
        private Quaternion _rotation;

        public override void PreparePose()
        {
            var pose = OptitrackListener.GetPose(TrackedName);
            if (pose != null)
            {
                _position = CalibrationParams.GetCalibratedPosition(pose.Position, pose.Rotation);
                //_rotation = CalibrationParams.GetCalibratedRotation(pose.Rotation);
                _rotation = pose.Rotation; // rotation is calibrated 'against' VR tracker
            }
        }

        public override Vector3 GetPosition()
        {
            return _position;
        }

        public override Quaternion GetRotation()
        {
            return _rotation;
        }

    }
}
