using Assets.Modules.Core;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphConnection : MonoBehaviour
    {
        public LineSegment LineTemplate;

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

        private LineSegment[] _lineSegments;

        void OnEnable()
        {
            StartGraph = UnityUtility.FindParent<Graph>(this);
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
                var scale = Mathf.Abs((_endGraph.Position + _endGraph.Width / 2) - (_startGraph.Position + _startGraph.Width / 2));
                transform.localScale = new Vector3(1, 1, scale);
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
            GenerateLines();
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
            GenerateLines();
        }


        private void OnStartGraphDataChange()
        {
            GenerateLines();
        }

        private void OnEndGraphDataChange()
        {
            GenerateLines();
        }

        private void GenerateLines()
        {
            ClearLines();

            if (_startGraph != null && _endGraph != null && _startGraph.Data != null && _endGraph.Data != null)
            {
                Debug.Assert(_startGraph.Data.Length == _endGraph.Data.Length);

                _lineSegments = new LineSegment[_startGraph.Data.Length];

                for (int i = 0; i < _startGraph.Data.Length; i++)
                {
                    var go = Instantiate(LineTemplate);
                    go.transform.parent = transform;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                    // reduce editor load (?)
                    go.gameObject.hideFlags = HideFlags.HideAndDontSave;

                    var segment = go.GetComponent<LineSegment>();
                    var startPoint = new Vector3(_startGraph.Data[i].ValueX, _startGraph.Data[i].ValueY, 0);
                    var endPoint = new Vector3(_endGraph.Data[i].ValueX, _endGraph.Data[i].ValueY, 1);
                    segment.SetPositions(startPoint, endPoint);

                    DataLineManager.GetLine(i).AddSegment(segment);
                    _lineSegments[i] = segment;
                }
            }
        }

        private void ClearLines()
        {
            if (_lineSegments != null)
            {
                for (int i = 0; i < _lineSegments.Length; i++)
                {
                    DataLineManager.GetLine(i).RemoveSegment(_lineSegments[i]);
                    var go = _lineSegments[i].gameObject;
                    Destroy(go);
                }

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
