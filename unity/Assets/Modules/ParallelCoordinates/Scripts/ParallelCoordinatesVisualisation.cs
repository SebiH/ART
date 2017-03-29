using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(SkinnedMeshLineRenderer))]
    public class ParallelCoordinatesVisualisation : MonoBehaviour
    {
        public GraphTracker LeftTracker;
        public GraphTracker RightTracker;

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
                    LeftTracker.TrackedGraph = _leftGraph;
                    LeftTracker.Track();
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
                    RightTracker.TrackedGraph = _rightGraph;
                    RightTracker.Track();
                    OnRightDataChange();
                }
            }
        }

        private SkinnedMeshLineRenderer _lineRenderer;

        private void OnEnable()
        {
            _lineRenderer = GetComponent<SkinnedMeshLineRenderer>();
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
                _hasLeftData = true;
                var data = _leftGraph.Graph.GetDataPosition();
                Debug.Assert(data.Length == _lineRenderer.Lines.Length);
                for (var i = 0; i < _lineRenderer.Lines.Length; i++)
                {
                    _lineRenderer.Lines[i].Start = data[i];
                }
            }

            UpdateRenderer(UpdateMode.Position);
        }

        private void OnRightDataChange()
        {
            if (_rightGraph.Graph.DimX == null || _rightGraph.Graph.DimY == null)
            {
                _hasRightData = false;
            }
            else
            {
                _hasRightData = true;
                var data = _rightGraph.Graph.GetDataPosition();
                Debug.Assert(data.Length == _lineRenderer.Lines.Length);
                for (var i = 0; i < _lineRenderer.Lines.Length; i++)
                {
                    _lineRenderer.Lines[i].End = data[i];
                }
            }

            UpdateRenderer(UpdateMode.Position);
        }

        public void SetColors(Color32[] colors)
        {
            for (var i = 0; i < _lineRenderer.Lines.Length; i++)
            {
                _lineRenderer.Lines[i].Color = colors[i];
            }

            UpdateRenderer(UpdateMode.Color);
        }


        private enum UpdateMode { Position, Color, Both }

        private void UpdateRenderer(UpdateMode mode)
        {
            if (_hasLeftData && _hasRightData)
            {
                LeftTracker.Track();
                RightTracker.Track();

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
