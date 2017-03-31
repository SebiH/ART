using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    public class GraphFilterListener : MonoBehaviour
    {
        public FilterRenderer FilterTemplate;

        private Surface _surface;
        private readonly List<FilterRenderer> _filters = new List<FilterRenderer>();

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
                    var filterGO = Instantiate(FilterTemplate);
                    Debug.Log(request.text);
                    _filters.Add(filterGO);
                    filterGO.Id = filter.id;
                    filterGO.RenderPath(filter.path);
                }
            }
        }

        private void HandleSurfaceAction(string cmd, string payload)
        {
            switch (cmd)
            {
                case "+filter":
                    {
                        var filterWrapper = JsonUtility.FromJson<RemoteFilterWrapper>(payload);
                        foreach (var filter in filterWrapper.filters)
                        {
                            var filterGO = Instantiate(FilterTemplate);
                            _filters.Add(filterGO);
                            filterGO.Id = filter.id;
                            filterGO.RenderPath(filter.path);
                        }
                    }
                    break;

                case "filter":
                    {
                        var filter = JsonUtility.FromJson<RemoteFilter>(payload);
                        var filterGO = _filters.FirstOrDefault(f => f.Id == filter.id);
                        if (filterGO) { filterGO.RenderPath(filter.path); }
                    }
                    break;

                case "-filter":
                    {
                        var id = int.Parse(payload);
                        var filterGO = _filters.FirstOrDefault(f => f.Id == id);
                        _filters.Remove(filterGO);
                        Destroy(filterGO.gameObject);
                    }
                    break;
            }
        }



        [Serializable]
        private class RemoteFilterWrapper
        {
            public RemoteFilter[] filters = null;
        }
    }

}
