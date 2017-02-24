using Assets.Modules.Core;
using Assets.Modules.Graphs;
using Assets.Modules.ParallelCoordinates;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    [RequireComponent(typeof(GraphManager), typeof(SurfaceGraphLayouter))]
    public class SurfaceGraphInterface : MonoBehaviour
    {
        private Surface _surface;
        private SurfaceGraphLayouter _layout;
        private GraphManager _graphManager;
        private RemoteDataProvider _dataProvider = new RemoteDataProvider();

        void OnEnable()
        {
            // search for Surface anchor on parents
            _surface = UnityUtility.FindParent<Surface>(this);
            _surface.OnAction += HandleSurfaceAction;

            _graphManager = GetComponent<GraphManager>();
            _layout = GetComponent<SurfaceGraphLayouter>();

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

                if (graphInfo.data.hasFilter) { DataLineManager.SetFilter(null); }
                else { DataLineManager.SetFilter(graphInfo.data.selectedDataIndices); }
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

                /*
                 * Graph updates are split into two channels:
                 * - graph-data for infrequent updates with big payload (due to array of selectionpolygon & selected ids)
                 * - graph-position with high-frequency positional data
                 */
                case "graph-data":
                    wrapper = JsonUtility.FromJson<RemoteGraphs>(payload);
                    foreach (var remoteGraph in wrapper.graphs)
                    {
                        var graph = _graphManager.GetGraph(remoteGraph.id);
                        if (graph) { UpdateGraphData(graph, remoteGraph); }
                    }
                    break;

                case "graph-position":
                    wrapper = JsonUtility.FromJson<RemoteGraphs>(payload);
                    foreach (var remoteGraph in wrapper.graphs)
                    {
                        var graph = _graphManager.GetGraph(remoteGraph.id);
                        if (graph) { UpdateGraphPosition(graph, remoteGraph); }
                    }
                    break;

                case "-graph":
                    var graphId = int.Parse(payload);
                    RemoveGraph(graphId);
                    break;

                case "selectedDataIndices":
                    var dataWrapper = JsonUtility.FromJson<DataWrapper>(payload);
                    if (dataWrapper.hasFilter) { DataLineManager.SetFilter(null); }
                    else { DataLineManager.SetFilter(dataWrapper.selectedDataIndices); }
                    break;
            }
        }

        private void AddGraph(RemoteGraph remoteGraph)
        {
            var hasDuplicate = (_graphManager.GetGraph(remoteGraph.id) != null);
            if (!hasDuplicate)
            {
                var graph = _graphManager.CreateGraph(remoteGraph.id);
                UpdateGraphData(graph, remoteGraph);
                UpdateGraphPosition(graph, remoteGraph);
            }
            else
            {
                Debug.LogWarning("Graph " + remoteGraph.id + " already exists!");
            }
        }

        private void UpdateGraphData(Graph graph, RemoteGraph remoteGraph)
        {
            graph.Color = remoteGraph.color;
            graph.IsSelected = remoteGraph.isSelected;
            graph.IsNewlyCreated = remoteGraph.isNewlyCreated;

            if (String.IsNullOrEmpty(remoteGraph.dimX))
            {
                graph.DimX = null;
            }
            else if (graph.DimX == null || graph.DimX.DisplayName != remoteGraph.dimX)
            {
                _dataProvider.LoadDataAsync(remoteGraph.dimX, (dim) =>
                {
                    graph.DimX = dim;
                });
            }

            if (String.IsNullOrEmpty(remoteGraph.dimY))
            {
                graph.DimY = null;
            }
            else if (graph.DimY == null || graph.DimY.DisplayName != remoteGraph.dimY)
            {
                _dataProvider.LoadDataAsync(remoteGraph.dimY, (dim) =>
                {
                    graph.DimY = dim;
                });
            }
        }


        private void UpdateGraphPosition(Graph graph, RemoteGraph remoteGraph)
        {
            graph.Position = _surface.PixelToUnityCoord(remoteGraph.pos);
            graph.Width = _surface.PixelToUnityCoord(remoteGraph.width);
        }


        private void RemoveGraph(int graphId)
        {
            _graphManager.RemoveGraph(graphId);
        }

        [Serializable]
        private class RemoteGraphs
        {
            public RemoteGraph[] graphs = new RemoteGraph[0];
            public DataWrapper data = null;
        }

        [Serializable]
        private class DataWrapper
        {
            public int[] selectedDataIndices = new int[0];
            public bool hasFilter = false;
        }
    }
}
