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
                    AddFilter(filter);
                }
            }
        }

        private void HandleSurfaceAction(string cmd, string payload)
        {
            switch (cmd)
            {
                case "+filter":
                    {
                        var filter = JsonUtility.FromJson<RemoteFilter>(payload);
                        AddFilter(filter);
                    }
                    break;

                case "filter":
                    {
                        var filterWrapper = JsonUtility.FromJson<RemoteFilterWrapper>(payload);
                        foreach (var filter in filterWrapper.filters)
                        {
                            UpdateFilter(filter);
                        }
                    }
                    break;

                case "-filter":
                    {
                        var id = int.Parse(payload);
                        RemoveFilter(id);
                    }
                    break;
            }
        }

        private void AddFilter(RemoteFilter rFilter)
        {
            var filter = _filters.FirstOrDefault(f => f.Id == rFilter.id);
            if (!filter)
            {
                filter = Instantiate(FilterTemplate);
                _filters.Add(filter);
                filter.Id = rFilter.id;
            }
            else
            {
                Debug.LogWarning("Tried to add already existing filter with id " + rFilter.id);
            }

            UpdateFilter(rFilter, filter);
        }

        private void UpdateFilter(RemoteFilter rFilter)
        {
            var filter = _filters.FirstOrDefault(f => f.Id == rFilter.id);

            if (!filter)
            {
                AddFilter(rFilter);
            }
            else
            {
                UpdateFilter(rFilter, filter);
            }
        }

        private void UpdateFilter(RemoteFilter rFilter, FilterRenderer filter)
        {
            filter.RenderPath(rFilter.path);
        }

        private void RemoveFilter(int id)
        {
            var filter = _filters.FirstOrDefault(f => f.Id == id);
            if (filter)
            {
                _filters.Remove(filter);
                Destroy(filter.gameObject);
            }
        }


        [Serializable]
        private class RemoteFilterWrapper
        {
            public RemoteFilter[] filters = new RemoteFilter[0];
        }
    }

}
