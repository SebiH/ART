using Assets.Modules.Tracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class DisplayMarker_SetupDisplay : MonoBehaviour
    {
        // Editor
        public DisplayMarker_PerformCalibration.MarkerOffset.Corner CurrentCorner;
        public string CalibratorName = "CalibrationHelper";
        public int CalibratorMarkerIndex = 4;


        // For other scripts
        public struct DisplayCorner
        {
            public DisplayMarker_PerformCalibration.MarkerOffset.Corner Corner;
            public Vector3 Position;
        }

        public List<DisplayCorner> CalibratedCorners { get; private set; }
        public float CalibrationProgress { get; private set; }
        public bool CanCalibrate { get; private set; }

        private bool _hasCalibratorPosition = false;
        private Vector3 _currentCalibratorPosition;

        void OnEnable()
        {
            CalibratedCorners = new List<DisplayCorner>();
            CalibrationProgress = 0;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
        }

        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == CalibratorName)
                {
                    foreach (var marker in pose.Markers)
                    {
                        if (marker.Id == CalibratorMarkerIndex)
                        {
                            if (!_hasCalibratorPosition)
                            {
                                CanCalibrate = true;
                            }

                            _currentCalibratorPosition = marker.Position;
                            _hasCalibratorPosition = true;
                        }
                    }
                }
            }
        }


        public void StartCalibration()
        {
            if (CanCalibrate)
            {
                StartCoroutine(Calibrate());
            }
        }

        public IEnumerator Calibrate()
        {
            var samples = 0;
            var maxSamples = 100;
            var totalPositions = Vector3.zero;

            CanCalibrate = false;
            CalibrationProgress = 0f;

            while (samples < maxSamples)
            {
                totalPositions += _currentCalibratorPosition;
                ++samples;
                CalibrationProgress = (float)samples / maxSamples;
                yield return new WaitForSeconds(0.01f);
            }

            var avgPosition = totalPositions / maxSamples;

            CalibratedCorners.Add(new DisplayCorner
            {
                Position = avgPosition,
                Corner = CurrentCorner
            });

            CanCalibrate = true;
        }


        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                if (_hasCalibratorPosition)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(_currentCalibratorPosition, 0.02f);
                }

                foreach (var cc in CalibratedCorners)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(cc.Position, 0.01f);
                }
            }
        }

    }
}
