using Assets.Modules.Core.Code;
using Assets.Modules.Tracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class DisplayMarker_SetupDisplay : MonoBehaviour
    {
        // Editor
        public string LoadFile = "default_corners.json";
        public string CalibratorName = "CalibrationHelper";
        public int CalibratorMarkerIndex = 4;
        public DisplayMarker_PerformCalibration.MarkerOffset.Corner CurrentCorner;

        // For other scripts
        [Serializable]
        public class DisplayCorner
        {
            public DisplayMarker_PerformCalibration.MarkerOffset.Corner Corner;
            public Vector3 Position;
        }

        public List<DisplayCorner> CalibratedCorners = new List<DisplayCorner>();
        public float CalibrationProgress { get; private set; }
        public bool CanCalibrate { get; private set; }

        private bool _hasCalibratorPosition = false;
        private Vector3 _currentCalibratorPosition;

        void OnEnable()
        {
            CalibrationProgress = 0;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;

            if (LoadFile.Length != 0)
            {
                LoadCalibratedCorners(LoadFile);
            }
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






        [Serializable]
        private class MarkerUnityWorkaround
        {
            public DisplayCorner[] array;
        }

        public void SaveCalibratedCorners(string relativeFilename)
        {
            MarkerUnityWorkaround workaround = new MarkerUnityWorkaround();
            workaround.array = CalibratedCorners.ToArray();

            var absoluteFilename = Paths.GetAbsolutePath(relativeFilename);
            File.WriteAllText(absoluteFilename, JsonUtility.ToJson(workaround));
            Debug.Log(String.Format("Saved {0} corners to {1}", workaround.array.Length, absoluteFilename));
        }

        public void LoadCalibratedCorners(string relativeFilename)
        {
            var absoluteFilename = Paths.GetAbsolutePath(relativeFilename);
            Debug.Log(String.Format("Loading from {0}", absoluteFilename));
            var contents = File.ReadAllText(absoluteFilename);
            var workaround = JsonUtility.FromJson<MarkerUnityWorkaround>(contents);

            foreach (var corner in workaround.array)
            {
                CalibratedCorners.Add(corner);
            }

            Debug.Log(String.Format("Loaded {0} Calibrated corners", workaround.array.Length));


        }
    }
}
