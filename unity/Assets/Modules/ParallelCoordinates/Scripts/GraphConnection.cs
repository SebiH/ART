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
        public Graph NextGraph { get { return _nextGraph; } }

        private GraphicsLineRenderer _lineRenderer;
        private List<LineSegment> _lineSegments = new List<LineSegment>();

        private bool _needsLineUpdate = false;
        private bool _hasCreatedLines = false;

        private void OnEnable()
        {
            _lineRenderer = GetComponent<GraphicsLineRenderer>();
            _originGraph = UnityUtility.FindParent<Graph>(this);
            _originGraph.OnDataChange += AdjustLines;
        }

        private void OnDisable()
        {
            SetNextGraph(null);
            ClearLines();
        }

        private void Update()
        {
            UpdateScale();
        }

        private void LateUpdate()
        {
            if (_needsLineUpdate && _nextGraph)
            {
                if (_hasCreatedLines)
                {
                    for (int i = 0; i < _lineSegments.Count; i++)
                    {
                        var segment = _lineSegments[i];
                        segment.DesiredStart = GetLineStart(i);
                        segment.DesiredEnd = GetLineEnd(i);
                    }
                }
                else
                {
                    CreateLines();
                }
            }
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
            if (_nextGraph && _hasCreatedLines)
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
                    var scale = (_nextGraph.transform.position) - (_originGraph.transform.position);
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scale.magnitude);
                }
            }
        }


        public void SetNextGraph(Graph graph)
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
                UpdateScale();
                _lineRenderer.ClearLines();
                CreateLines();
                graph.OnDataChange += AdjustLines;
            }
            else // switching out graphs
            {
                graph.OnDataChange += AdjustLines;
                _needsLineUpdate = true;
            }
        }

        private void AdjustLines()
        {
            _needsLineUpdate = true;
        }

        public void SwapWithNext()
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
            if (_hasCreatedLines)
            {
                AdjustLines();
            }
            else if (_originGraph.Data != null)
            {
                _hasCreatedLines = true;
                for (int i = 0; i < _originGraph.Data.Length; i++)
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


        public static GraphConnection Get(Graph graph)
        {
            return graph.GetComponentInChildren<GraphConnection>();
        }


        void OnDrawGizmosSelected()
        {
            if (_nextGraph)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _nextGraph.transform.position);
            }
        }
    }
}
