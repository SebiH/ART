using System;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class MultiMarker_MarkerSetup : MonoBehaviour
    {
        public Vector3 CalibratorToMarkerPosOffset = Vector3.zero;
        public Vector3 CalibratorToMarkerRotOffset = Vector3.zero;

        public int BindToIndex = -1;

        public GameObject Calibrator;
        public GameObject MarkerPreviewPrefab;
        private GameObject _markerPreview;

        void OnEnable()
        {
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


        public void SetMarker()
        {
            // Wait 10 seconds to gather average of all optitrack points?
        }
    }
}
