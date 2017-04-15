using Assets.Modules.Core;
using Assets.Modules.Graphs;
using Assets.Modules.SurfaceGraphs;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    [RequireComponent(typeof(GraphManager), typeof(SurfaceGraphInterface))]
    public class GraphFilterListener : MonoBehaviour
    {
        public FilterRenderer FilterTemplate;

        private Surface _surface;
        private GraphManager _graphManager;
        private readonly List<FilterRenderer> _filters = new List<FilterRenderer>();
        private readonly List<RemoteFilter> _remoteFilters = new List<RemoteFilter>();

        private void OnEnable()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
            _surface.OnAction += HandleSurfaceAction;

            _graphManager = GetComponent<GraphManager>();

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

            var graphInterface = GetComponent<SurfaceGraphInterface>();
            while (!graphInterface.IsInitialized)
            {
                yield return new WaitForEndOfFrame();
            }

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
            var graph = _graphManager.GetGraph(rFilter.origin);
            if (graph)
            {
                if (!filter)
                {
                    filter = Instantiate(FilterTemplate);
                    filter.Init(graph);

                    _filters.Add(filter);
                    filter.Id = rFilter.id;
                }
                else
                {
                    Debug.LogWarning("Tried to add already existing filter with id " + rFilter.id);
                }

                UpdateFilter(rFilter, filter);
            }
            else
            {
                Debug.LogWarning("Tried to add filter for non-existing graph " + rFilter.id);
            }
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
            _remoteFilters.RemoveAll(rf => rf.id == rFilter.id);
            _remoteFilters.Add(rFilter);

            var color = new Color(3f/255f, 169f/255f, 244f/255f, 1);
            if (!String.IsNullOrEmpty(rFilter.color))
            {
                var colorSuccess = ColorUtility.TryParseHtmlString(rFilter.color, out color);
                if (!colorSuccess)
                {
                    Debug.LogWarning("Could not parse color " + rFilter.color);
                }
            }

            if (rFilter.type == RemoteFilter.Type.Metric)
            {
                if (rFilter.gradient != null && filter.Gradients == null)
                {
                    filter.Gradients = ConvertGradient(rFilter.gradient);
                    filter.GradientAxis = rFilter.boundDimensions == "x" ? 'x' : 'y';
                }

                UpdateGradientLimits(rFilter.origin, rFilter.boundDimensions);
            }
            else
            {
                filter.Color = color;
            }

            filter.Path = rFilter.path;
        }

        private void UpdateGradientLimits(int origin, string axis)
        {
            // Gradients are based on all similar filters for one graph
            // If, for example, a graph has one metric filter both sides,
            // the gradient will span between both filters.
            // If the graph has only one filter, this filter will use
            // the whole gradient
            var similarFilters = _remoteFilters.FindAll(f =>
                f.origin == origin &&
                f.type == RemoteFilter.Type.Metric &&
                f.boundDimensions == axis &&
                f.range != null
            );

            if (similarFilters.Count > 0)
            {
                var min = similarFilters.Min(f => f.range[0]);
                var max = similarFilters.Max(f => f.range[1]);
                // similarFilters includes rFilter
                foreach (var sf in similarFilters)
                {
                    var goSf = _filters.FirstOrDefault(f => f.Id == sf.id);
                    if (goSf)
                    {
                        goSf.MinGradient = min;
                        goSf.MaxGradient = max;
                    }
                }
            }
        }

        private FilterRenderer.GradientStop[] ConvertGradient(RemoteFilter.GradientStop[] gradients)
        {
            return gradients.Select(g => new FilterRenderer.GradientStop(g.stop, g.color)).ToArray();
        }

        private void RemoveFilter(int id)
        {
            var filter = _filters.FirstOrDefault(f => f.Id == id);
            if (filter)
            {
                _filters.Remove(filter);
                Destroy(filter.gameObject);
            }

            var rFilter = _remoteFilters.FirstOrDefault(f => f.id == id);
            if (rFilter != null)
            {
                _remoteFilters.Remove(rFilter);
                if (rFilter.type == RemoteFilter.Type.Metric)
                {
                    UpdateGradientLimits(rFilter.origin, rFilter.boundDimensions);
                }
            }
        }


        [Serializable]
        private class RemoteFilterWrapper
        {
            public RemoteFilter[] filters = new RemoteFilter[0];
        }
    }

}
