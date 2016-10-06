using Assets.Modules.Tracking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class MultiMarker_MarkerSetup : MonoBehaviour
    {
        public Vector3 CalibratorToMarkerPosOffset = Vector3.zero;
        public Vector3 CalibratorToMarkerRotOffset = Vector3.zero;

        public int BindToIndex = -1;

        public string OptitrackCalibratorName = "CalibrationHelper";
        public GameObject MarkerPreviewPrefab;
        public GameObject MarkerPrefab;
        private GameObject _markerPreview;
        private int _currentCalibratingMarkerId;

        public bool CanSetMarker { get; private set; }
        public float SetMarkerProgress { get; private set; }

        void OnEnable()
        {
            CanSetMarker = true;
            SetMarkerProgress = 0f;

            _markerPreview = GameObject.Instantiate(MarkerPreviewPrefab);

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

                    _markerPreview.transform.position += CalibratorToMarkerPosOffset;
                    _markerPreview.transform.rotation = pose.Rotation * Quaternion.Euler(CalibratorToMarkerRotOffset);

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
            var rotationSamples = Vector3.zero;
            int sampleCount = 0;

            CanSetMarker = false;

            while (SetMarkerProgress < 1f)
            {
                sampleCount++;
                positionSamples += _markerPreview.transform.position;
                //rotationSamples += _markerPreview.tran

                SetMarkerProgress += 0.05f;
                yield return new WaitForSeconds(0.1f);
            }

            if (sampleCount > 0)
            {
                positionSamples = positionSamples / sampleCount;
            }

            var createdMarker = Instantiate(MarkerPrefab);
            createdMarker.GetComponent<MultiMarker_Debug_CameraIndicator>().MarkerId = _currentCalibratingMarkerId;
            createdMarker.transform.position = positionSamples;
            // TODO: rotation sampling!
            createdMarker.transform.rotation = _markerPreview.transform.rotation;

            CanSetMarker = true;
            SetMarkerProgress = 0f;
        }
    }
}
