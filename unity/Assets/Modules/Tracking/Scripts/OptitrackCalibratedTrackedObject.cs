using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class OptitrackCalibratedTrackedObject : MonoBehaviour
    {
        // Name in OptiTrack
        public string TrackedName = "";

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnPosesReceived;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnPosesReceived;
        }

        void OnPosesReceived(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == TrackedName)
                {
                    transform.position = pose.Position + pose.Rotation * CalibrationParams.OptitrackToCameraOffset;
                    break;
                }
            }
        }

    }
}
