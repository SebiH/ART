using Assets.Modules.Core;
using Assets.Modules.Graphs;
using Assets.Modules.ParallelCoordinates;
using Assets.Modules.Surfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceGraph
{
    [RequireComponent(typeof(GraphManager), typeof(GraphConnectionManager))]
    public class SurfaceGraphInterface : MonoBehaviour
    {
        private Surface _surface;
        private GraphManager _graphManager;
        private GraphConnectionManager _connectionManager;

        private readonly List<GraphInfo> _currentGraphs = new List<GraphInfo>();

        void OnEnable()
        {
            // search for Surface anchor on parents
            _surface = UnityUtility.FindParent<Surface>(this);
            _surface.OnAction += HandleSurfaceAction;

            _graphManager = GetComponent<GraphManager>();
            _connectionManager = GetComponent<GraphConnectionManager>();

            // TODO1 fetch initial graph data
        }

        void OnDisable()
        {
            _surface.OnAction -= HandleSurfaceAction;
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
                    break;

                case "graph-data":
                    wrapper = JsonUtility.FromJson<GraphInfoWrapper>(payload);
                    foreach (var i in wrapper.graphs)
                    {
                        UpdateGraphData(i);
                    }
                    break;

                case "graph-position":
                    wrapper = JsonUtility.FromJson<GraphInfoWrapper>(payload);
                    foreach (var i in wrapper.graphs)
                    {
                        UpdateGraphPosition(i);
                    }
                    break;

                case "-graph":
                    var graphId = int.Parse(payload);
                    RemoveGraph(graphId);
                    break;
            }
        }

        private GraphInfo GetExistingGraphInfo(int id)
        {
            return _currentGraphs.FirstOrDefault(g => g.id == id);
        }

        private void AddGraph(GraphInfo graphInfo)
        {
            _graphManager.CreateGraph(graphInfo.id);

            var existingGraph = GetExistingGraphInfo(graphInfo.id);
            if (existingGraph == null)
            {
                _currentGraphs.Add(graphInfo);
            }
            else
            {
                Debug.LogWarning("Already have graphinfo for id " + graphInfo.id);
            }

            UpdateGraphData(graphInfo);
            UpdateGraphPosition(graphInfo);
        }

        private void UpdateGraphData(GraphInfo graphInfo)
        {
            var graph = _graphManager.GetGraph(graphInfo.id);

            if (graphInfo.dimX != null && graphInfo.dimY != null)
            {
                graph.SetData(graphInfo.dimX, graphInfo.dimY);
            }

            // TODO1 selectedData && isSelected
        }

        private void UpdateGraphPosition(GraphInfo graphInfo)
        {
            var graph = _graphManager.GetGraph(graphInfo.id);
            graph.transform.localPosition += new Vector3(0, 0, -graph.transform.localPosition.z + _surface.PixelToUnityCoord(graphInfo.pos));

            var nextGraphInfo = _currentGraphs.Where((g) => g.index > graphInfo.index).OrderBy(g => g.index).FirstOrDefault();

            if (nextGraphInfo != null)
            {
                var nextGraph = _graphManager.GetGraph(nextGraphInfo.id);
                var connection = _connectionManager.SetConnection(graph, nextGraph);
            }
        }

        private void RemoveGraph(int graphId)
        {
            var graphInfo = GetExistingGraphInfo(graphId);

            if (graphInfo != null)
            {
                _currentGraphs.Remove(graphInfo);
            }

            _connectionManager.RemoveConnection(graphId);
            _graphManager.RemoveGraph(graphId);
        }


        [Serializable]
        private class GraphInfoWrapper
        {
            public GraphInfo[] graphs = new GraphInfo[0];
        }
    }
}
