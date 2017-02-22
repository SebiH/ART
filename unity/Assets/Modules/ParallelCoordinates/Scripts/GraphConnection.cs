using Assets.Modules.Core;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(GraphicsLineRenderer))]
    public class GraphConnection : MonoBehaviour
    {
        private LineSegment[] _lineSegments;
        private GraphicsLineRenderer _lineRenderer;
        // avoids multiple line updates due to overlapping events
        private bool _needsLineUpdate = false;

        private Graph _startGraph = null;
        public Graph StartGraph
        {
            get { return _startGraph; } 
            set { if (value != _startGraph) { SetStartGraph(value); } }
        }

        private Graph _endGraph = null;
        public Graph EndGraph
        {
            get { return _endGraph; } 
            set { if (value != _endGraph) { SetEndGraph(value); } }
        }

        void OnEnable()
        {
            StartGraph = UnityUtility.FindParent<Graph>(this);
            _lineRenderer = GetComponent<GraphicsLineRenderer>();
        }
        
        void OnDisable()
        {
            // unsubscribe from any events
            SetStartGraph(null);
            SetEndGraph(null);
        }

        void Update()
        {
            if (_startGraph && _endGraph)
            {
                var scale = (_endGraph.transform.position) - (_startGraph.transform.position);
                transform.localScale = new Vector3(1, 1, scale.magnitude);
            }

            if (_needsLineUpdate)
            {
                GenerateLines();
                _needsLineUpdate = false;
            }
        }


        private void SetStartGraph(Graph graph)
        {
            if (graph == _startGraph)
            {
                return;
            }

            if (_startGraph)
            {
                _startGraph.OnDataChange -= OnStartGraphDataChange;
            }

            if (graph)
            {
                graph.OnDataChange += OnStartGraphDataChange;
            }

            _startGraph = graph;
            _needsLineUpdate = true;
        }

        private void SetEndGraph(Graph graph)
        {
            if (graph == _endGraph)
            {
                return;
            }

            if (_endGraph)
            {
                _endGraph.OnDataChange -= OnEndGraphDataChange;
            }

            if (graph)
            {
                graph.OnDataChange += OnEndGraphDataChange;
            }

            _endGraph = graph;
            _needsLineUpdate = true;
        }


        private void OnStartGraphDataChange()
        {
            _needsLineUpdate = true;
        }

        private void OnEndGraphDataChange()
        {
            _needsLineUpdate = true;
        }

        private void GenerateLines()
        {
            if (_startGraph != null && _endGraph != null && _startGraph.Data != null && _endGraph.Data != null)
            {
                Debug.Assert(_startGraph.Data.Length == _endGraph.Data.Length);
                var dataLength = _startGraph.Data.Length;

                if (_lineSegments != null && _lineSegments.Length == dataLength)
                {
                    AdjustLines();
                }
                else
                {
                    ClearLines();
                    InitializeLines();
                }
            }
            else
            {
                ClearLines();
            }
        }

        private void InitializeLines()
        {
            _lineSegments = new LineSegment[_startGraph.Data.Length];

            for (int i = 0; i < _lineSegments.Length; i++)
            {
                var segment = new LineSegment();
                _lineSegments[i] = segment;
                DataLineManager.GetLine(i).AddSegment(segment);

                segment.DesiredStart = new Vector3(_startGraph.Data[i].ValueX, _startGraph.Data[i].ValueY, 0);
                segment.DesiredEnd = new Vector3(_endGraph.Data[i].ValueX, _endGraph.Data[i].ValueY, 1);

                segment.SetRenderer(_lineRenderer);
            }
        }

        private void AdjustLines()
        {
            for (int i = 0; i < _lineSegments.Length; i++)
            {
                var segment = _lineSegments[i];
                segment.DesiredStart = new Vector3(_startGraph.Data[i].ValueX, _startGraph.Data[i].ValueY, 0);
                segment.DesiredEnd = new Vector3(_endGraph.Data[i].ValueX, _endGraph.Data[i].ValueY, 1);
            }
        }


        private void ClearLines()
        {
            if (_lineSegments != null)
            {
                for (int i = 0; i < _lineSegments.Length; i++)
                {
                    DataLineManager.GetLine(i).RemoveSegment(_lineSegments[i]);
                }

                _lineRenderer.ClearLines();

                _lineSegments = null;
            }
        }




        void OnDrawGizmosSelected()
        {
            if (_endGraph)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _endGraph.transform.position);
            }
        }
    }
}
