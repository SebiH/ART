using Assets.Modules.Core;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(SkinnedMeshLineRenderer))]
    public class ParallelCoordinatesVisualisation : MonoBehaviour
    {
        private GraphManager _graphManager;
        private Graph _originGraph;
        private Graph _connectedGraph;

        private SkinnedMeshLineRenderer _lineRenderer;

        private void OnEnable()
        {
            _graphManager = UnityUtility.FindParent<GraphManager>(this);
            _originGraph = UnityUtility.FindParent<Graph>(this);
            _originGraph.OnDataChange += HandleDataChange;

            _lineRenderer = GetComponent<SkinnedMeshLineRenderer>();
            _lineRenderer.SetHidden(true);
        }

        private void OnDisable()
        {

        }


        private void LateUpdate()
        {
            var newConnectedGraph = FindConnectedGraph();
            if (newConnectedGraph != _connectedGraph)
            {
                _connectedGraph = newConnectedGraph;

                if (_connectedGraph == null)
                {
                    _lineRenderer.SetHidden(true);
                }
                else
                {
                    _lineRenderer.SetHidden(false);
                }
            }
        }


        private GraphPosition FindConnectedGraph()
        {
            GraphPosition newConnectedGraph = null;
            foreach (var graph in _graphManager.GetAllGraphs())
            {
                var isEligible = !graph.IsNewlyCreated;
                var isNext = graph.Position > _originGraph.Position;
                var isNearest = (newConnectedGraph == null || newConnectedGraph.Position > graph.Position);

                if (isEligible && isNext && isNearest)
                {
                    newConnectedGraph = graph;
                }
            }
            return newConnectedGraph;
        }


    }
}
