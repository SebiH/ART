using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ArMarkerSpawner : MonoBehaviour
    {
        public ArMarker MarkerTemplate;

        private List<ArMarker> _markers = new List<ArMarker>();
        private float _markerSize = 0.25f;

        void OnEnable()
        {

        }

        void OnDisable()
        {

        }

        public ArMarker CreateMarker(int id)
        {
            // need to set properties first before OnEnable() is called
            MarkerTemplate.gameObject.SetActive(false);
            var marker = Instantiate(MarkerTemplate);

            marker.Id = id;
            marker.Size = _markerSize;
            marker.transform.parent = transform;
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localRotation = Quaternion.Euler(90, 0, 0);
            marker.transform.localScale = Vector3.one;
            marker.name = "Marker_" + id;

            // reactivate with all properties set for OnEnable()
            marker.gameObject.SetActive(true);
            _markers.Add(marker);

            return marker;
        }

        public void RemoveMarker(int id)
        {
            var marker = GetMarker(id);
            if (marker)
            {
                _markers.Remove(marker);
                Destroy(marker.gameObject);
            }
        }

        public ArMarker GetMarker(int id)
        {
            return _markers.FirstOrDefault(m => m.Id == id);
        }

        public void SetMarkerSize(float size)
        {
            if (Mathf.Abs(_markerSize - size) < Mathf.Epsilon)
            {
                return;
            }

            foreach (var marker in _markers)
            {
                marker.Size = size;
            }
        }

        public void ClearMarkers()
        {
            while (_markers.Count > 0)
            {
                RemoveMarker(_markers[0].Id);
            }
        }
    }
}
