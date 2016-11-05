using Assets.Modules.Core;
using Assets.Modules.Tracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class MultiMarker_MarkerSetup : MonoBehaviour
    {
        public struct CalibratedMarkerObject
        {
            public int Id;
            public GameObject Marker;
        }


        public Vector3 CalibratorToMarkerPosOffset = Vector3.zero;
        public Vector3 CalibratorToMarkerRotOffset = Vector3.zero;

        public int BindToIndex = -1;

        public bool LoadDefaultMarkers = false;

        public string OptitrackCalibratorName = "CalibrationHelper";
        public GameObject MarkerPreviewPrefab;
        public GameObject MarkerPrefab;
        private GameObject _markerPreview;
        private int _currentCalibratingMarkerId;

        public bool CanSetMarker { get; private set; }
        public float SetMarkerProgress { get; private set; }


        public List<CalibratedMarkerObject> CalibratedMarkers
        {
            get; private set;
        }

        void OnEnable()
        {
            CalibratedMarkers = new List<CalibratedMarkerObject>();

            if (LoadDefaultMarkers)
            {
                LoadCalibratedMarkers("default_markers.json");
            }

            CanSetMarker = true;
            SetMarkerProgress = 0f;

            _markerPreview = Instantiate(MarkerPreviewPrefab);

            var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;
            _markerPreview.transform.localScale = new Vector3(markerSize, 0.001f, markerSize);

            OptitrackListener.Instance.PosesReceived += OnOptitrackPoses;
        }

        void OnDisable()
        {
            Destroy(_markerPreview);
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPoses;
        }


        void OnOptitrackPoses(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCalibratorName)
                {
                    if (BindToIndex == -1)
                    {
                        _markerPreview.transform.position = pose.Position;
                    }
                    else
                    {
                        var marker = pose.Markers.Find((m) => m.Id == BindToIndex);
                        _markerPreview.transform.position = marker.Position;
                    }

                    _markerPreview.transform.rotation = Quaternion.Euler(CalibratorToMarkerRotOffset) * pose.Rotation;
                    _markerPreview.transform.position += _markerPreview.transform.TransformDirection(CalibratorToMarkerPosOffset);

                    break;
                }
            }
        }

        public void StartSetMarker(int markerId)
        {
            if (CanSetMarker)
            {
                _currentCalibratingMarkerId = markerId;
                StartCoroutine(SetMarker());
            }
        }

        private IEnumerator SetMarker()
        {
            var positionSamples = Vector3.zero;
            var rotationSamples = new List<Quaternion>();
            int sampleCount = 0;

            CanSetMarker = false;

            while (SetMarkerProgress < 1f)
            {
                sampleCount++;
                positionSamples += _markerPreview.transform.position;
                rotationSamples.Add(_markerPreview.transform.rotation);

                SetMarkerProgress += 0.05f;
                yield return new WaitForSeconds(0.1f);
            }

            var avgRotation = Quaternion.identity;
            if (sampleCount > 0)
            {
                positionSamples = positionSamples / sampleCount;
                avgRotation = MathUtility.Average(rotationSamples);
            }

            var createdMarker = Instantiate(MarkerPrefab);
            createdMarker.name = String.Format("Marker_{0}", _currentCalibratingMarkerId);
            createdMarker.GetComponent<MultiMarker_Debug_CameraIndicator>().MarkerId = _currentCalibratingMarkerId;
            createdMarker.transform.position = positionSamples;
            createdMarker.transform.rotation = avgRotation;

            CanSetMarker = true;
            SetMarkerProgress = 0f;

            CalibratedMarkers.Add(new CalibratedMarkerObject
            {
                Id = _currentCalibratingMarkerId,
                Marker = createdMarker
            });
        }


        [Serializable]
        private class SerializableCalibratedMarker
        {
            public int Id;
            public Vector3 Position;
            public Quaternion Rotation;
        }

        [Serializable]
        private class MarkerUnityWorkaround
        {
            public SerializableCalibratedMarker[] array;
        }

        public void SaveCalibratedMarkers(string filename)
        {
            MarkerUnityWorkaround workaround = new MarkerUnityWorkaround();
            workaround.array = new SerializableCalibratedMarker[CalibratedMarkers.Count];

            int i = 0;
            foreach (var calibratedMarker in CalibratedMarkers)
            {
                workaround.array[i] = new SerializableCalibratedMarker
                {
                    Id = calibratedMarker.Id,
                    Position = calibratedMarker.Marker.transform.position,
                    Rotation = calibratedMarker.Marker.transform.rotation
                };
                ++i;
            }

            FileUtility.SaveToFile(filename, JsonUtility.ToJson(workaround));
        }

        public void LoadCalibratedMarkers(string filename)
        {
            var contents = FileUtility.LoadFromFile(filename);
            var markers = JsonUtility.FromJson<MarkerUnityWorkaround>(contents);

            foreach (var marker in markers.array)
            {
                var createdMarker = Instantiate(MarkerPrefab);
                createdMarker.name = String.Format("Marker_{0}", marker.Id);
                createdMarker.GetComponent<MultiMarker_Debug_CameraIndicator>().MarkerId = marker.Id;
                createdMarker.transform.position = marker.Position;
                createdMarker.transform.rotation = marker.Rotation;

                CalibratedMarkers.Add(new CalibratedMarkerObject
                {
                    Id = marker.Id,
                    Marker = createdMarker
                });
            }

            Debug.Log(String.Format("Loaded {0} Calibrated markers", markers.array.Length));
        }
    }
}
