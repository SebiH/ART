using Assets.Modules.Core;
using Assets.Modules.ParallelCoordinates;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    public class GlobalFilterListener : MonoBehaviour
    {
        const byte TRANSPARENCY_FILTERED = 0;
        const byte TRANSPARENCY_NORMAL = 255;

        private Surface _surface;

        private void OnEnable()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
            _surface.OnAction += HandleSurfaceAction;

            StartCoroutine(InitWebData());
        }

        private void OnDisable()
        {
            _surface.OnAction -= HandleSurfaceAction;
        }

        private IEnumerator InitWebData()
        {
            var request = new WWW(String.Format("{0}:{1}/api/filter/global", Globals.SurfaceServerIp, Globals.SurfaceWebPort));
            yield return request;

            if (request.text != null && request.text.Length > 0)
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<RemoteValueMetadataWrapper>(request.text);
                    ApplyMetadata(wrapper.globalfilter);
                }
                catch (Exception)
                {
                    // filter not initialised yet
                }
            }
        }

        private void HandleSurfaceAction(string cmd, string payload)
        {
            if (cmd == "globalfilter")
            {
                var wrapper = JsonUtility.FromJson<RemoteValueMetadataWrapper>(payload);
                ApplyMetadata(wrapper.globalfilter);
            }
        }

        private void ApplyMetadata(RemoteValueMetadata[] metadata)
        {
            var colors = new Color32[metadata.Length];

            for (var i = 0; i < colors.Length; i++)
            {
                var color = new Color();
                var parseSuccess = ColorUtility.TryParseHtmlString(metadata[i].c, out color);

                if (parseSuccess)
                {
                    colors[i] = color;
                }
                else
                {
                    Debug.LogWarning("Unable to parse color " + metadata[i].c);
                }

                colors[i].a = (metadata[i].f >= 1) ? TRANSPARENCY_FILTERED : TRANSPARENCY_NORMAL;
            }

            if (colors.Length > 0)
            {
                ParallelCoordinatesManager.Instance.SetColors(colors);
            }
        }


        [Serializable]
        private class RemoteValueMetadataWrapper
        {
            public RemoteValueMetadata[] globalfilter = null;
        }
    }
}
