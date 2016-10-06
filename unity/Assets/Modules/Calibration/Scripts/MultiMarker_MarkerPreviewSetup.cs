using UnityEngine;
using Assets.Modules.Tracking;
using System.Collections.Generic;

namespace Assets.Modules.Calibration
{
    public class MultiMarker_MarkerPreviewSetup : MonoBehaviour
    {
        public int AnchorId = -1;
        public int Span1Id = -1;
        public int Span2Id = -1;
        public int ForwardId = -1;

        public string OptitrackCalibratorName = "CalibrationHelper";

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnOptitrackPoses;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPoses;
        }

        private void OnOptitrackPoses(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCalibratorName)
                {
                    CalibrateMarker(pose);
                    enabled = false;
                }
            }
        }

        private void CalibrateMarker(OptitrackPose pose)
        {
            var markerSetup = GetComponent<MultiMarker_MarkerSetup>();
            

        }
    }
}
