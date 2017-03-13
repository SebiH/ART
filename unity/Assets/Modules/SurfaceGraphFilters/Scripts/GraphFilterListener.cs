using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    public class GraphFilterListener : MonoBehaviour
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
            var request = new WWW(String.Format("{0}:{1}/api/filter/list", Globals.SurfaceServerIp, Globals.SurfaceWebPort));
            yield return request;

            if (request.text != null && request.text.Length > 0)
            {
                var filterWrapper = JsonUtility.FromJson<RemoteFilterWrapper>(request.text);
                foreach (var filter in filterWrapper.filters)
                {
                    // TODO
                }
            }
        }

        private void HandleSurfaceAction(string cmd, string payload)
        {
            switch (cmd)
            {
                case "+filter":
                    break;

                case "filter":
                    break;

                case "-filter":
                    break;
            }
        }



        private class RemoteFilterWrapper
        {
            public RemoteFilter[] filters;
        }
    }

}
