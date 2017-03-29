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
        private Surface _surface;
        private GraphManager _graphManager;
        private RemoteDataProvider _dataProvider = new RemoteDataProvider();

        void OnEnable()
        {
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

            if (request.text != null && request.text.Length > 0)
            {
                var graphInfo = JsonUtility.FromJson<RemoteGraphs>(request.text);
                foreach (var graph in graphInfo.graphs)
                {
                    AddGraph(graph);
                }
            }
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
                graph.Layout.Init(remoteGraph.pos, 0.5f, 0.5f);
                graph.Layout.Scale = Vector3.one * 0.7f;
                graph.gameObject.SetActive(true);
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
            else if (g.Graph.DimX == null || g.Graph.DimX.DisplayName != remoteGraph.dimX)
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
            else if (g.Graph.DimY == null || g.Graph.DimY.DisplayName != remoteGraph.dimY)
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
