using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(SkinnedMeshLineRenderer))]
    public class ParallelCoordinatesVisualisation : MonoBehaviour
    {
        private bool _hasLeftData = false;
        private GraphMetaData _leftGraph;
        public GraphMetaData Left
        {
            get { return _leftGraph; }
            set
            {
                if (_leftGraph != value)
                {
                    if (_leftGraph)
                    {
                        _leftGraph.Graph.OnDataChange -= OnLeftDataChange;
                    }

                    Debug.Assert(value != null);
                    _leftGraph = value;
                    _leftGraph.Graph.OnDataChange += OnLeftDataChange;
                    OnLeftDataChange();
                }
            }
        }

        private bool _hasRightData = false;
        private GraphMetaData _rightGraph;
        public GraphMetaData Right
        {
            get { return _rightGraph; }
            set
            {
                if (_rightGraph != value)
                {
                    if (_rightGraph)
                    {
                        _rightGraph.Graph.OnDataChange -= OnRightDataChange;
                    }

                    Debug.Assert(value != null);
                    _rightGraph = value;
                    _rightGraph.Graph.OnDataChange += OnRightDataChange;
                    OnRightDataChange();
                }
            }
        }

        private SkinnedMeshLineRenderer _lineRenderer;

        private void OnEnable()
        {
            _lineRenderer = GetComponent<SkinnedMeshLineRenderer>();
            _lineRenderer.SetHidden(true);
        }

        private void OnDisable()
        {
            if (_leftGraph)
            {
                _leftGraph.Graph.OnDataChange -= OnLeftDataChange;
            }

            if (_rightGraph)
            {
                _rightGraph.Graph.OnDataChange -= OnRightDataChange;
            }
        }

        private void OnLeftDataChange()
        {
            if (_leftGraph.Graph.DimX == null || _leftGraph.Graph.DimY == null)
            {
                _hasLeftData = false;
            }
            else
            {
                // TODO: update data
            }

            UpdateRenderer(UpdateMode.Position);
        }

        private void OnRightDataChange()
        {
            if (_leftGraph.Graph.DimX == null || _leftGraph.Graph.DimY == null)
            {
                _hasLeftData = false;
            }
            else
            {
                // TODO: update data
            }

            UpdateRenderer(UpdateMode.Position);
        }

        private enum UpdateMode { Position, Color, Both }

        private void UpdateRenderer(UpdateMode mode)
        {
            if (_hasLeftData && _hasRightData)
            {
                switch (mode)
                {
                    case UpdateMode.Both:
                        _lineRenderer.GenerateMesh();
                        break;
                    case UpdateMode.Position:
                        _lineRenderer.UpdatePositions();
                        break;
                    case UpdateMode.Color:
                        _lineRenderer.UpdateColors();
                        break;
                }

                _lineRenderer.SetHidden(false);
            }
            else
            {
                _lineRenderer.SetHidden(true);
            }
        }
    }
}
