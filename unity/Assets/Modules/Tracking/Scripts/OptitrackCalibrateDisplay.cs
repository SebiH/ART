using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class OptitrackCalibrateDisplay : MonoBehaviour
    {
        // Set within editor
        public Corner CurrentCorner;
        public string CalibratorName = "CalibrationHelper";
        public int CalibratorMarkerIndex = 4;

        public int MaxSamples = 100;
        public float MaxPoseLag = 0.1f;

        public string DisplayName = "Surface";

        public float BorderTop;
        public float BorderLeft;
        public float BorderRight;
        public float BorderBottom;
        public float HeightOffset;

        // for use within menu
        public void SetCornerTopLeft() { CurrentCorner = Corner.TopLeft; }
        public void SetCornerTopRight() { CurrentCorner = Corner.TopRight; }
        public void SetCornerBottomLeft() { CurrentCorner = Corner.BottomLeft; }
        public void SetCornerBottomRight() { CurrentCorner = Corner.BottomRight; }

        public float CalibrationProgress { get; private set; }
        public bool IsCalibrating { get; private set; }

        private OptitrackPose _calibratorPose;
        private Vector3[] _calibratedCorners = new Vector3[4];
        private bool[] _isCornerCalibrated = new bool[4];

        void OnEnable()
        {
            IsCalibrating = false;
            CalibrationProgress = 0;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
        }


        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            var matchingPose = poses.FirstOrDefault((p) => p.RigidbodyName == CalibratorName);
            
            if (matchingPose != null)
            {
                _calibratorPose = matchingPose;
            }
        }


        public void StartCalibration()
        {
            if (!IsCalibrating && _calibratorPose != null)
            {
                StartCoroutine(CalibrateCorner());
            }
        }

        private IEnumerator CalibrateCorner()
        {
            var samples = 0;
            var totalPositions = Vector3.zero;

            IsCalibrating = true;
            CalibrationProgress = 0f;

            while (samples < MaxSamples)
            {
                if (Time.unscaledTime - _calibratorPose.DetectionTime > MaxPoseLag)
                {
                    Debug.Log("Outdated Optitrack Pose, waiting for new pose");
                    yield return new WaitForSeconds(0.01f);
                    continue;
                }

                var pos = _calibratorPose.Position;
                var marker = _calibratorPose.Markers.FirstOrDefault((m) => m.Id == CalibratorMarkerIndex);
                if (marker != null)
                {
                    pos = marker.Position;
                }

                totalPositions += pos;
                ++samples;
                CalibrationProgress = (float)samples / MaxSamples;
                yield return new WaitForSeconds(0.01f);
            }

            var avgPosition = totalPositions / MaxSamples;
            var cornerIndex = (int)CurrentCorner;
            _calibratedCorners[cornerIndex] = avgPosition;
            _isCornerCalibrated[cornerIndex] = true;

            IsCalibrating = false;
        }

        public void CommitFixedDisplay()
        {
            if (!_isCornerCalibrated.All((b) => b))
            {
                Debug.Log("Not all cornes calibrated yet");
                return;
            }

            FixedDisplays.Set(DisplayName,
                _calibratedCorners[(int)Corner.TopLeft] + new Vector3(BorderLeft, HeightOffset, -BorderTop),
                _calibratedCorners[(int)Corner.BottomLeft] + new Vector3(BorderLeft, HeightOffset, BorderBottom),
                _calibratedCorners[(int)Corner.BottomRight] + new Vector3(-BorderRight, HeightOffset, BorderBottom),
                _calibratedCorners[(int)Corner.TopRight] + new Vector3(-BorderRight, HeightOffset, -BorderTop));

            for (int i = 0; i < _isCornerCalibrated.Length; i++)
            {
                _isCornerCalibrated[i] = false;
            }
        }


        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                if (_calibratorPose != null)
                {
                    var calibPosition = _calibratorPose.Position;
                    var marker = _calibratorPose.Markers.FirstOrDefault((m) => m.Id == CalibratorMarkerIndex);
                    if (marker != null)
                    {
                        calibPosition = marker.Position;
                    }

                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(calibPosition, 0.02f);
                }

                for (int i = 0; i < _isCornerCalibrated.Length; i++)
                {
                    if (_isCornerCalibrated[i])
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireSphere(_calibratedCorners[i], 0.01f);
                    }
                }
            }
        }
    }
}
