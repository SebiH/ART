using Assets.Modules.Calibration;
using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using Assets.Modules.Tracking;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface
{
    [RequireComponent(typeof(ArMarkerSpawner))]
    public class SurfaceMarkerInterface : MonoBehaviour
    {
        private Surface _surface;
        private ArMarkerSpawner _markerSpawner;

        void OnEnable()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
            _surface.OnAction += HandleSurfaceAction;

            _markerSpawner = GetComponent<ArMarkerSpawner>();

            StartCoroutine(InitWebData());
        }

        void OnDisable()
        {
            _surface.OnAction -= HandleSurfaceAction;
        }

        private IEnumerator InitWebData()
        {
            var request = new WWW(String.Format("{0}:{1}/api/marker/list", Globals.SurfaceServerIp, Globals.SurfaceWebPort));
            yield return request;

            if (request.text != null && request.text.Length > 0)
            {
                var markerInfos = JsonUtility.FromJson<MarkerInfoWrapper>(request.text);
                foreach (var markerInfo in markerInfos.markers)
                {
                    UpdateMarker(markerInfo);
                }
            }
        }

        private void HandleSurfaceAction(string command, string payload)
        {
            switch (command)
            {
                case "+marker":
                    UpdateMarker(JsonUtility.FromJson<MarkerInfo>(payload));
                    break;

                case "marker":
                    var markerInfos = JsonUtility.FromJson<MarkerInfoWrapper>(payload);
                    foreach (var markerInfo in markerInfos.markers)
                    {
                        UpdateMarker(markerInfo);
                    }
                    break;

                case "-marker":
                    RemoveMarker(int.Parse(payload));
                    break;

                case "marker-clear":
                    ClearMarkers();
                    break;
            }
        }

        private void UpdateMarker(MarkerInfo markerInfo)
        {
            var marker = _markerSpawner.GetMarker(markerInfo.id);
            if (!marker)
            {
                marker = _markerSpawner.CreateMarker(markerInfo.id);
            }

            var markerSize = _surface.PixelToUnityCoord(markerInfo.size);
            _markerSpawner.SetMarkerSize(markerSize);
            ArMarkerTracker.Instance.MarkerSizeInMeter = markerInfo.size;

            var posX = _surface.PixelToUnityCoord(markerInfo.posX);
            var posY = _surface.PixelToUnityCoord(_surface.DisplayResolution.height - markerInfo.posY);
            marker.Position = new Vector2(posX, posY);
        }

        private void RemoveMarker(int markerId)
        {
            _markerSpawner.RemoveMarker(markerId);
        }

        private void ClearMarkers()
        {
            _markerSpawner.ClearMarkers();
        }


        [Serializable]
        private struct MarkerInfoWrapper
        {
            public MarkerInfo[] markers;
        }
    }
}
