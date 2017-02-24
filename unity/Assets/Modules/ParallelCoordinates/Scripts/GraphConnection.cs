using Assets.Modules.Core;
using Assets.Modules.Graphs;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(GraphicsLineRenderer))]
    public class GraphConnection : MonoBehaviour
    {
        private Graph _originGraph;
        private Graph _nextGraph;

        private GraphManager _graphManager;
        private GraphicsLineRenderer _lineRenderer;
        private List<LineSegment> _lineSegments = new List<LineSegment>();

        private bool _needsLineUpdate = false;
        private bool _hasCreatedLines = false;

        private void OnEnable()
        {
            _graphManager = UnityUtility.FindParent<GraphManager>(this);
            _lineRenderer = GetComponent<GraphicsLineRenderer>();
            _originGraph = UnityUtility.FindParent<Graph>(this);
            _originGraph.OnDataChange += AdjustLines;
            UpdateScale();
        }

        private void OnDisable()
        {
            SetNextGraph(null);
            ClearLines();
        }

        private void Update()
        {
            UpdateScale();
            FindNextGraph();
        }

        private void LateUpdate()
        {
            if (_needsLineUpdate)
            {
                if (_nextGraph && _nextGraph.HasData)
                {
                    CreateLines();
                }
                else
                {
                    ClearLines();
                }

                _needsLineUpdate = false;
            }
        }

        private void FindNextGraph()
        {
            Graph nextGraph = null;
            foreach (var graph in _graphManager.GetAllGraphs())
            {
                var isEligible = !graph.IsNewlyCreated;
                var isNext = graph.Position > _originGraph.Position;
                var isNearest = (nextGraph == null || nextGraph.Position > graph.Position);

                if (isEligible && isNext && isNearest)
                {
                    nextGraph = graph;
                }
            }

            SetNextGraph(nextGraph);
        }

        private Vector3 GetLineStart(int index)
        {
            return _originGraph.GetLocalCoordinates(index);
        }

        private Vector3 GetLineEnd(int index)
        {
            return transform.InverseTransformPoint(_nextGraph.GetWorldCoordinates(index));
        }

        private void UpdateScale()
        {
            if (_nextGraph)
            {
                if (_nextGraph.IsAnimating || _originGraph.IsAnimating)
                {
                    for (int i = 0; i < _lineSegments.Count; i++)
                    {
                        var line = _lineSegments[i];
                        line.Start = GetLineStart(i);
                        line.End = GetLineEnd(i);
                    }
                }
                else
                {
                    var scale = ((_nextGraph.transform.position) - (_originGraph.transform.position)).magnitude;

                    if (Mathf.Abs(scale) > Mathf.Epsilon)
                    {
                        transform.localScale = new Vector3(1, 1, scale);
                    }
                }
            }
        }


        private void SetNextGraph(Graph graph)
        {
            var prevGraph = _nextGraph;
            _nextGraph = graph;
            if (prevGraph == graph)
            {
                return;
            }

            if (prevGraph)
            {
                prevGraph.OnDataChange -= AdjustLines; 
            }

            if (graph == null)
            {
                _lineRenderer.ClearLines();
                _hasCreatedLines = false;
            }
            else if (prevGraph == null)
            {
                _lineRenderer.ClearLines();

                if (graph.HasData)
                {
                    CreateLines();
                }

                graph.OnDataChange += AdjustLines;
            }
            else // switching out graphs
            {
                graph.OnDataChange += AdjustLines;
                AdjustLines();
            }
        }

        private void AdjustLines()
        {
            _needsLineUpdate = true;
        }

        private void SwapWithNext()
        {
            if (_nextGraph == null)
            {
                Debug.Assert(false, "Shouldn't swap with empty graph!");
                return;
            }

            var nextConnection = GetComponentInChildren<GraphConnection>();
            nextConnection._lineRenderer.SwapWith(_lineRenderer);

            SetNextGraph(_nextGraph);
            nextConnection.SetNextGraph(_originGraph);
        }

        private void CreateLines()
        {
            UpdateScale();

            if (_hasCreatedLines)
            {
                for (int i = 0; i < _lineSegments.Count; i++)
                {
                    var segment = _lineSegments[i];
                    segment.DesiredStart = GetLineStart(i);
                    segment.DesiredEnd = GetLineEnd(i);
                }
            }
            else if (_originGraph.HasData && _nextGraph && _nextGraph.HasData)
            {
                _hasCreatedLines = true;
                for (int i = 0; i < _originGraph.DataLength; i++)
                {
                    var segment = new LineSegment();
                    segment.SetRenderer(_lineRenderer);
                    _lineSegments.Add(segment);
                    DataLineManager.GetLine(i).AddSegment(segment);

                    segment.Start = GetLineStart(i);
                    segment.End = GetLineEnd(i);
                }
            }
        }


        private void ClearLines()
        {
            for (int i = 0; i < _lineSegments.Count; i++)
            {
                DataLineManager.GetLine(i).RemoveSegment(_lineSegments[i]);
            }
            _lineSegments.Clear();
            _lineRenderer.ClearLines();
            _hasCreatedLines = false;
        }


        void OnDrawGizmosSelected()
        {
            if (_nextGraph)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _nextGraph.transform.position);
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.05f);
            }
        }
    }
}
