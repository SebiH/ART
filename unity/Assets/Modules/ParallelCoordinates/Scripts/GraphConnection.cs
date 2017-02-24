using Assets.Modules.Core;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(GraphicsLineRenderer))]
    public class GraphConnection : MonoBehaviour
    {
        private Graph _originGraph;
        private Graph _connectedGraph;

        private GraphManager _graphManager;

        private LineSegment[] _lines = null;
        private GraphicsLineRenderer _lineRenderer;

        private void OnEnable()
        {
            _graphManager = UnityUtility.FindParent<GraphManager>(this);
            _originGraph = UnityUtility.FindParent<Graph>(this);
            _originGraph.OnDataChange += HandleDataChange;
            _lineRenderer = GetComponent<GraphicsLineRenderer>();
            _lineRenderer.SetHidden(true);
        }

        private void OnDisable()
        {
            _originGraph.OnDataChange -= HandleDataChange;
            SetConnectedGraph(null);

            if (_lines != null)
            {
                for (int i = 0; i < _lines.Length; i++)
                {
                    DataLineManager.GetLine(i).RemoveSegment(_lines[i]);
                }
                _lines = null;
            }
        }

        private void Update()
        {
            var newConnectedGraph = FindConnectedGraph();
            if (newConnectedGraph != _connectedGraph)
            {
                SetConnectedGraph(newConnectedGraph);
            }

            UpdateScale();
        }


        private Graph FindConnectedGraph()
        {
            Graph newConnectedGraph = null;
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

        private void SetConnectedGraph(Graph newGraph)
        {
            if (_connectedGraph != null)
            {
                _connectedGraph.OnDataChange -= HandleDataChange;
            }

            if (newGraph != null)
            {
                newGraph.OnDataChange += HandleDataChange;
            }

            // TODO: possibly swap meshes

            _connectedGraph = newGraph;
            GenerateLines(true);
        }

        private void HandleDataChange()
        {
            GenerateLines(true);
        }

        private void GenerateLines(bool animate)
        {
            UpdateScale();

            var hasData = (_connectedGraph != null && _originGraph.HasData && _connectedGraph.HasData);
            if (!hasData)
            {
                _lineRenderer.SetHidden(true);
            }
            else
            {
                if (_lineRenderer.IsHidden())
                {
                    _lineRenderer.SetHidden(false);
                    animate = false;
                }

                // micro optimisations to use pipelining as much as possible - incoming code duplication!
                if (_lines == null)
                {
                    Debug.Assert(_originGraph.DataLength == _connectedGraph.DataLength);
                    _lines = new LineSegment[_originGraph.DataLength];

                    for (int i = 0; i < _lines.Length; i++)
                    {
                        var line = new LineSegment();
                        line.SetRenderer(_lineRenderer);
                        _lines[i] = line;
                        DataLineManager.GetLine(i).AddSegment(line);

                        line.Start = GetLineStart(i);
                        line.End = GetLineEnd(i);
                    }
                }
                else
                {
                    if (animate)
                    {
                        for (int i = 0; i < _lines.Length; i++)
                        {
                            _lines[i].DesiredStart = GetLineStart(i);
                            _lines[i].DesiredEnd = GetLineEnd(i);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _lines.Length; i++)
                        {
                            _lines[i].Start = GetLineStart(i);
                            _lines[i].End = GetLineEnd(i);
                        }
                    }
                }
            }
        }

        private void UpdateScale()
        {
            if (_connectedGraph != null && !_connectedGraph.IsAnimating && !_originGraph.IsAnimating)
            {
                transform.localScale = new Vector3(1, 1, _originGraph.Position - _connectedGraph.Position);
            }
        }

        private Vector3 GetLineStart(int index)
        {
            return _originGraph.GetLocalCoordinates(index);
        }

        private Vector3 GetLineEnd(int index)
        {
            return transform.InverseTransformPoint(_connectedGraph.GetWorldCoordinates(index));
        }

        void OnDrawGizmosSelected()
        {
            if (_connectedGraph)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _connectedGraph.transform.position);
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.05f);
            }
        }
    }
}
