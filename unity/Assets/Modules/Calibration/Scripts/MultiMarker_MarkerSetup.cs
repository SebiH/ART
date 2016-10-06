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

        public GameObject Calibrator;
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
        }

        void OnDisable()
        {
            Destroy(_markerPreview);
        }

        void Update()
        {
            _markerPreview.transform.localPosition = CalibratorToMarkerPosOffset;
            _markerPreview.transform.localRotation = Quaternion.Euler(CalibratorToMarkerRotOffset);

            if (BindToIndex == -1)
            {
                _markerPreview.transform.parent = Calibrator.transform;
            }
            else
            {
                // TODO: code duplication..
                var optitrackMarker = Calibrator.transform.Find(String.Format("Marker_{0}", BindToIndex + 1));
                if (optitrackMarker != null)
                {
                    _markerPreview.transform.parent = optitrackMarker;
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
