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
    [RequireComponent(typeof(GraphManager))]
    public class SurfaceGraphInterface : MonoBehaviour
    {
        private Surface _surface;
        private GraphManager _graphManager;

        private readonly List<GraphInfo> _currentGraphs = new List<GraphInfo>();
        private readonly List<GraphConnection> _graphConnections = new List<GraphConnection>();

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
            }
        }

        private GraphInfo GetExistingGraphInfo(int id)
        {
            return _currentGraphs.FirstOrDefault(g => g.id == id);
        }

        private void AddGraph(GraphInfo graphInfo)
        {
            var graph = _graphManager.CreateGraph(graphInfo.id);

            Debug.Assert(graph.GetComponent<GraphConnection>() != null, "GraphTemplate used by GraphManager must have GraphConnection script attached");
            _graphConnections.Add(graph.GetComponent<GraphConnection>());

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
                    existingGraph.selectedData = updatedGraph.selectedData;
                    existingGraph.isSelected = updatedGraph.isSelected;
                }
            }

            foreach (var updatedGraph in graphInfoWrapper.graphs)
            {
                UpdateGraphData(updatedGraph);
            }

            // TODO1 isSelected
            DataLineManager.ClearFilter();

            foreach (var gi in _currentGraphs)
            {
                if (gi.selectedData != null && gi.selectedData.Length > 0)
                {
                    DataLineManager.AddFilter(gi.selectedData);
                }
            }

            DataLineManager.ApplyFilter();
        }

        private void UpdateGraphData(GraphInfo graphInfo)
        {
            var graph = _graphManager.GetGraph(graphInfo.id);
            graph.SetData(graphInfo.dimX, graphInfo.dimY);
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
            graph.transform.localPosition += new Vector3(0, 0, -graph.transform.localPosition.z + _surface.PixelToUnityCoord(graphInfo.pos));

            var nextGraphInfo = _currentGraphs.FirstOrDefault(g => g.id == graphInfo.nextId);

            if (nextGraphInfo != null)
            {
                var nextGraph = _graphManager.GetGraph(nextGraphInfo.id);
                var graphConnection = _graphConnections.FirstOrDefault(c => c.StartGraph != null && c.StartGraph.Id == graphInfo.id);
                if (graphConnection)
                {
                    graphConnection.EndGraph = nextGraph;
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
        }
    }
}
