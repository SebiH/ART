using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    public class GlobalFilterListener : MonoBehaviour
    {
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
                var wrapper = JsonUtility.FromJson<RemoteValueMetadataWrapper>(request.text);
                foreach (var metadata in wrapper.globalFilter)
                {
                    // TODO
                }
            }
        }

        private void HandleSurfaceAction(string cmd, string payload)
        {
            if (cmd == "globalfilter")
            {

            }
        }



        private class RemoteValueMetadataWrapper
        {
            public RemoteValueMetadata[] globalFilter;
        }
    }
}
