using Assets.Modules.Core;
using Assets.Modules.Graphs;
using Assets.Modules.ParallelCoordinates;
using Assets.Modules.Surfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface
{
    [RequireComponent(typeof(GraphManager), typeof(SurfaceGraphLayouter))]
    public class SurfaceGraphInterface : MonoBehaviour
    {
        private Surface _surface;
        private SurfaceGraphLayouter _layout;
        private GraphManager _graphManager;

        private readonly List<GraphInfo> _currentGraphs = new List<GraphInfo>();
        private readonly List<GraphConnection> _graphConnections = new List<GraphConnection>();

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
            _currentGraphs.Clear();
            _graphConnections.Clear();
        }

        private IEnumerator InitWebData()
        {
            var request = new WWW(String.Format("{0}:{1}/api/graph/list", Globals.SurfaceServerIp, Globals.SurfaceWebPort));
            yield return request;

            if (request.text != null && request.text.Length > 0)
            {
                var graphInfo = JsonUtility.FromJson<GraphInfoWrapper>(request.text);
                foreach (var graph in graphInfo.graphs)
                {
                    AddGraph(graph);
                }

                foreach (var graph in graphInfo.graphs)
                {
                    UpdateGraphData(graph);
                    UpdateGraphPosition(graph);
                }

                // create all graph/line objects
                yield return new WaitForEndOfFrame();

                if (graphInfo.data.hasFilter) { DataLineManager.SetFilter(null); }
                else { DataLineManager.SetFilter(graphInfo.data.selectedDataIndices); }

                UpdateGraphSelection();
            }
        }

        private void HandleSurfaceAction(string command, string payload)
        {
            GraphInfo info;
            GraphInfoWrapper wrapper;

            switch (command)
            {
                case "+graph":
                    info = JsonUtility.FromJson<GraphInfo>(payload);
                    AddGraph(info);
                    UpdateGraphData(info);
                    UpdateGraphPosition(info);
                    break;

                /*
                 * Graph updates are split into two channels:
                 * - graph-data for infrequent updates with big payload (due to array of selectionpolygon & selected ids)
                 * - graph-position with high-frequency positional data
                 */
                case "graph-data":
                    wrapper = JsonUtility.FromJson<GraphInfoWrapper>(payload);
                    UpdateGraphData(wrapper);
                    break;

                case "graph-position":
                    wrapper = JsonUtility.FromJson<GraphInfoWrapper>(payload);
                    UpdateGraphPosition(wrapper);
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

        private GraphInfo GetExistingGraphInfo(int id)
        {
            return _currentGraphs.FirstOrDefault(g => g.id == id);
        }

        private void AddGraph(GraphInfo graphInfo)
        {
            var graph = _graphManager.CreateGraph(graphInfo.id);
            var graphConnection = graph.GetComponentInChildren<GraphConnection>();

            Debug.Assert(graphConnection != null, "GraphTemplate used by GraphManager must have GraphConnection script in children");
            _graphConnections.Add(graphConnection);

            var existingGraphInfo = GetExistingGraphInfo(graphInfo.id);
            if (existingGraphInfo == null)
            {
                _currentGraphs.Add(graphInfo);
            }
            else
            {
                Debug.LogWarning("Already have graphinfo for id " + graphInfo.id);
            }
        }

        private void UpdateGraphData(GraphInfoWrapper graphInfoWrapper)
        {
            // update existing graph info first, before applying changes to actual graphs
            foreach (var updatedGraph in graphInfoWrapper.graphs)
            {
                var existingGraph = GetExistingGraphInfo(updatedGraph.id);
                if (existingGraph == null)
                {
                    AddGraph(updatedGraph);
                }
                else
                {
                    existingGraph.color = updatedGraph.color;
                    existingGraph.dimX = updatedGraph.dimX;
                    existingGraph.dimY = updatedGraph.dimY;
                    existingGraph.isSelected = updatedGraph.isSelected;
                }
            }

            foreach (var updatedGraph in graphInfoWrapper.graphs)
            {
                UpdateGraphData(updatedGraph);
            }

            UpdateGraphSelection();
        }

        private void UpdateGraphSelection()
        {
            var hasSelectedGraph = _currentGraphs.Any(g => g.isSelected);
            _layout.IsGraphSelected = hasSelectedGraph;
        }

        private void UpdateGraphData(GraphInfo graphInfo)
        {
            var graph = _graphManager.GetGraph(graphInfo.id);
            graph.SetData(graphInfo.dimX, graphInfo.dimY);

            var graphVisualisation = graph.GetComponentInChildren<GraphVisualisation>();
            Debug.Assert(graphVisualisation, "Graphs must have GraphVisualisation in children!");
            graphVisualisation.SetDimensionX(graphInfo.dimX, 0, 1);
            graphVisualisation.SetDimensionY(graphInfo.dimY, 0, 1);
        }

        private void UpdateGraphPosition(GraphInfoWrapper graphInfoWrapper)
        {
            // update existing graph info first, before applying changes to actual graphs
            foreach (var updatedGraph in graphInfoWrapper.graphs)
            {
                var existingGraph = GetExistingGraphInfo(updatedGraph.id);
                if (existingGraph == null)
                {
                    AddGraph(updatedGraph);
                }
                else
                {
                    existingGraph.pos = updatedGraph.pos;
                    existingGraph.nextId = updatedGraph.nextId;
                }
            }


            foreach (var updatedGraph in graphInfoWrapper.graphs)
            {
                UpdateGraphPosition(updatedGraph);
            }
        }

        private void UpdateGraphPosition(GraphInfo graphInfo)
        {
            var graph = _graphManager.GetGraph(graphInfo.id);
            graph.Position = _surface.PixelToUnityCoord(graphInfo.pos);
            graph.Width = _surface.PixelToUnityCoord(graphInfo.width);

            var nextGraphInfo = _currentGraphs.FirstOrDefault(g => g.id == graphInfo.nextId);

            var graphConnection = _graphConnections.FirstOrDefault(c => c.StartGraph != null && c.StartGraph.Id == graphInfo.id);
            if (graphConnection)
            {
                if (nextGraphInfo != null)
                {
                    var nextGraph = _graphManager.GetGraph(nextGraphInfo.id);
                    graphConnection.EndGraph = nextGraph;
                }
                else
                {
                    graphConnection.EndGraph = null;
                }
            }
        }

        private void RemoveGraph(int graphId)
        {
            var graphInfo = GetExistingGraphInfo(graphId);

            if (graphInfo != null)
            {
                _currentGraphs.Remove(graphInfo);
                var prevGraphConnection = _graphConnections.FirstOrDefault(c => c.EndGraph != null && c.EndGraph.Id == graphInfo.id);
                var currGraphConnection = _graphConnections.FirstOrDefault(c => c.StartGraph != null && c.StartGraph.Id == graphInfo.id);
                if (currGraphConnection && prevGraphConnection)
                {
                    prevGraphConnection.EndGraph = currGraphConnection.EndGraph;
                }
                else if (prevGraphConnection)
                {
                    prevGraphConnection.EndGraph = null;
                }
            }

            _graphManager.RemoveGraph(graphId);
        }


        [Serializable]
        private class GraphInfoWrapper
        {
            public GraphInfo[] graphs = new GraphInfo[0];
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
