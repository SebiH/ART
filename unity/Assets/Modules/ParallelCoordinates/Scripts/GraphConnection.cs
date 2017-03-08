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

        private bool _useFastMode = false;

        private void OnEnable()
        {
            _graphManager = UnityUtility.FindParent<GraphManager>(this);
            _originGraph = UnityUtility.FindParent<Graph>(this);
            _originGraph.OnDataChange += HandleDataChange;
            _lineRenderer = GetComponent<GraphicsLineRenderer>();
            _lineRenderer.SetHidden(true);

            UpdateScale();
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

        private void LateUpdate()
        {
            var newConnectedGraph = FindConnectedGraph();
            if (newConnectedGraph != _connectedGraph)
            {
                SetConnectedGraph(newConnectedGraph);
            }
            else if (_connectedGraph && !_useFastMode)
            {
                GenerateLines(false);
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

            _connectedGraph = newGraph;
            GenerateLines(false);
        }

        private void HandleDataChange()
        {
            GenerateLines(true);
        }

        private void GenerateLines(bool animate)
        {
            UpdateScale(true);
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
                    //if (animate)
                    //{
                    //    for (int i = 0; i < _lines.Length; i++)
                    //    {
                    //        _lines[i].DesiredStart = GetLineStart(i);
                    //        _lines[i].DesiredEnd = GetLineEnd(i);
                    //    }
                    //}
                    //else
                    //{
                        for (int i = 0; i < _lines.Length; i++)
                        {
                            _lines[i].Start = GetLineStart(i);
                            _lines[i].End = GetLineEnd(i);
                        }
                    //}
                }
            }
        }

        private void UpdateScale(bool suppressLineGeneration = false)
        {
            var prevFastMode = _useFastMode;
            transform.localScale = Vector3.one;
            _useFastMode = false;

            if (_connectedGraph != null)
            {
                var hasSameRotation = Mathf.Abs(Quaternion.Angle(_connectedGraph.transform.localRotation, _originGraph.transform.localRotation)) <= Mathf.Epsilon;
                var deltaPosition = _originGraph.transform.localPosition - _connectedGraph.transform.localPosition;
                var isAligned = (Mathf.Abs(deltaPosition.y) < Mathf.Epsilon && Mathf.Abs(deltaPosition.z) < Mathf.Epsilon);
                
                if (hasSameRotation && isAligned)
                {
                    transform.localScale = new Vector3(1, 1, -deltaPosition.x);
                    _useFastMode = true;
                }
            }

            if (prevFastMode != _useFastMode && !suppressLineGeneration)
            {
                GenerateLines(false);
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
