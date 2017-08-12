using Assets.Modules.Core;
using Assets.Modules.Graphs;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    [RequireComponent(typeof(GraphManager))]
    public class SurfaceGraphInterface : MonoBehaviour
    {
        public bool IsInitialized { get; private set; }

        private Surface _surface;
        private GraphManager _graphManager;
        private RemoteDataProvider _dataProvider = new RemoteDataProvider();

        void OnEnable()
        {
            IsInitialized = false;
            // search for Surface anchor on parents
            _surface = UnityUtility.FindParent<Surface>(this);
            _surface.OnAction += HandleSurfaceAction;

            _graphManager = GetComponent<GraphManager>();

            StartCoroutine(InitWebData());
        }

        void OnDisable()
        {
            _surface.OnAction -= HandleSurfaceAction;
        }

        private IEnumerator InitWebData()
        {
            var request = new WWW(String.Format("{0}:{1}/api/graph/list", Globals.SurfaceServerIp, Globals.SurfaceWebPort));
            yield return request;

            WebRequestHelper.WebResult result;
            WebRequestHelper.Instance.PerformWebRequest("graphs", request, out result);

            if (result.text != null && result.text.Length > 0)
            {
                var graphInfo = JsonUtility.FromJson<RemoteGraphs>(result.text);
                foreach (var graph in graphInfo.graphs)
                {
                    AddGraph(graph);
                }
            }

            IsInitialized = true;
        }

        private void HandleSurfaceAction(string command, string payload)
        {
            RemoteGraph info;
            RemoteGraphs wrapper;

            switch (command)
            {
                case "+graph":
                    info = JsonUtility.FromJson<RemoteGraph>(payload);
                    AddGraph(info);
                    break;
                case "graph":
                    wrapper = JsonUtility.FromJson<RemoteGraphs>(payload);
                    foreach (var remoteGraph in wrapper.graphs)
                    {
                        var graph = _graphManager.GetGraph(remoteGraph.id);
                        if (graph) { UpdateGraph(graph, remoteGraph); }
                        else { AddGraph(remoteGraph); }
                    }
                    break;

                case "-graph":
                    var graphId = int.Parse(payload);
                    RemoveGraph(graphId);
                    break;
            }
        }

        private void AddGraph(RemoteGraph remoteGraph)
        {
            var hasDuplicate = (_graphManager.GetGraph(remoteGraph.id) != null);
            if (!hasDuplicate)
            {
                GraphMetaData graph;

                if (remoteGraph.isNewlyCreated)
                {
                    graph = _graphManager.SpawnGraph(remoteGraph.id);
                }
                else
                {
                    graph = _graphManager.CreateGraph(remoteGraph.id);
                }

                UpdateGraph(graph, remoteGraph);
                graph.Layout.Init(_surface.PixelToUnityCoord(remoteGraph.pos), -0.5f, 0.5f);
            }
            else
            {
                Debug.LogWarning("Graph " + remoteGraph.id + " already exists!");
            }
        }

        private void UpdateGraph(GraphMetaData g, RemoteGraph remoteGraph)
        {
            g.Graph.IsColored = remoteGraph.isColored;
            g.Graph.IsSelected = remoteGraph.isSelected;
            g.Graph.IsFlipped = remoteGraph.isFlipped;
            g.Graph.IsPickedUp = remoteGraph.isPickedUp;
            g.Graph.SortAxis = remoteGraph.sortAxis;

            if (remoteGraph.sortIncline) {
                GraphMetaData prevGraph = null;

                foreach (var graph in _graphManager.GetAllGraphs())
                {
                    if (graph.Layout.Position < g.Layout.Position)
                    {
                        if (prevGraph == null || prevGraph.Layout.Position < graph.Layout.Position)
                        {
                            prevGraph = graph;
                        }
                    }
                }

                if (prevGraph)
                {
                    g.Graph.SortInclineHack(prevGraph.Graph.DimY, false);
                    g.Graph.SortIncline = true;
                }
            }
            else if (remoteGraph.sortInclineNextHack)
            {
                GraphMetaData nextGraph = null;

                foreach (var graph in _graphManager.GetAllGraphs())
                {
                    if (graph.Layout.Position > g.Layout.Position)
                    {
                        if (nextGraph == null || nextGraph.Layout.Position > graph.Layout.Position)
                        {
                            nextGraph = graph;
                        }
                    }
                }

                if (nextGraph)
                {
                    g.Graph.SortInclineHack(nextGraph.Graph.DimY, true);
                    g.Graph.SortIncline = true;
                }
            }
            else
            {
                g.Graph.SortIncline = false;
            }

            if (g.Graph.IsNewlyCreated && !remoteGraph.isNewlyCreated)
            {
                _graphManager.RegisterGraph(g);
            }

            g.Graph.IsNewlyCreated = remoteGraph.isNewlyCreated;

            var color = new Color();
            var colorSuccess = ColorUtility.TryParseHtmlString(remoteGraph.color, out color);
            if (colorSuccess) { g.Graph.Color = color; }
            else { Debug.LogWarning("Could not parse color " + remoteGraph.color); }

            if (String.IsNullOrEmpty(remoteGraph.dimX))
            {
                g.Graph.DimX = null;
            }
            else if (g.Graph.DimX == null || g.Graph.DimX.Name != remoteGraph.dimX)
            {
                _dataProvider.LoadDataAsync(remoteGraph.dimX, (dim) =>
                {
                    g.Graph.DimX = dim;
                });
            }

            if (String.IsNullOrEmpty(remoteGraph.dimY))
            {
                g.Graph.DimY = null;
            }
            else if (g.Graph.DimY == null || g.Graph.DimY.Name != remoteGraph.dimY)
            {
                _dataProvider.LoadDataAsync(remoteGraph.dimY, (dim) =>
                {
                    g.Graph.DimY = dim;
                });
            }

            g.Layout.Position = _surface.PixelToUnityCoord(remoteGraph.pos);
            g.Layout.Width = _surface.PixelToUnityCoord(remoteGraph.width);
        }


        private void RemoveGraph(int graphId)
        {
            _graphManager.RemoveGraph(graphId);
        }

        [Serializable]
        private class RemoteGraphs
        {
            public RemoteGraph[] graphs = new RemoteGraph[0];
        }
    }
}
