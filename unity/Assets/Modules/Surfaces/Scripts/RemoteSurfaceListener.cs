using Assets.Modules.Core;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    [RequireComponent(typeof(Surface))]
    public class RemoteSurfaceListener : MonoBehaviour
    {
        private Surface _surface;

        void OnEnable()
        {
            _surface = GetComponent<Surface>();
            _surface.OnAction += OnSurfaceAction;

            StartCoroutine(LoadInitialData());
        }

        void OnDisable()
        {
            _surface.OnAction -= OnSurfaceAction;
        }

        private void OnSurfaceAction(string cmd, string payload)
        {
            if (cmd == "surface")
            {
                var properties = JsonUtility.FromJson<SurfaceDataPayload>(payload);
                ApplyProperties(properties);
            }
        }

        private IEnumerator LoadInitialData()
        {
            var form = new WWWForm();
            form.AddField("name", _surface.ClientName);
            var request = new WWW(String.Format("{0}:{1}/api/surface", Globals.SurfaceServerIp, Globals.SurfaceWebPort), form);
            yield return request;

            if (request.text != null && request.text.Length > 0)
            {
                var surfaceData = JsonUtility.FromJson<SurfaceDataPayload>(request.text);
                ApplyProperties(surfaceData);
            }
        }

        private void ApplyProperties(SurfaceDataPayload properties)
        {
            var resolution = new Resolution
            {
                width = properties.width,
                height = properties.height
            };
            _surface.DisplayResolution = resolution;
            _surface.PixelToCmRatio = Math.Max(properties.pixelToCmRatio, float.Epsilon);
            _surface.Offset = properties.offset;
        }

        [Serializable]
        private class SurfaceDataPayload
        {
            public string name = "";
            public float pixelToCmRatio = 1;
            public int width = 0;
            public int height = 0;
            public float offset;
        }
    }
}
