using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(SkinnedMeshLineRenderer))]
    public class ParallelCoordinatesVisualisation : MonoBehaviour
    {
        const float LINE_ANIMATION_LENGTH = 0.5f;

        protected static float[] RandomOffsetX = null;
        protected static float[] RandomOffsetY = null;
        static ParallelCoordinatesVisualisation()
        {
            RandomOffsetX = new float[Globals.DataPointsCount];
            RandomOffsetY = new float[Globals.DataPointsCount];
            for (var i = 0; i < RandomOffsetX.Length; i++)
            {
                RandomOffsetX[i] = (Random.value - 0.5f) / 70f;
                RandomOffsetY[i] = (Random.value - 0.5f) / 70f;
            }
        }



        public GraphTracker LeftTracker;
        public GraphTracker RightTracker;

        private Vec2ArrayAnimation _leftAnimation = new Vec2ArrayAnimation(LINE_ANIMATION_LENGTH);
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

                    _leftGraph = value;

                    if (_leftGraph == null)
                    {
                        _hasLeftData = false;
                        _lineRenderer.SetHidden(true);
                    }
                    else
                    {
                        _leftGraph.Graph.OnDataChange += OnLeftDataChange;
                        LeftTracker.TrackedGraph = _leftGraph;
                        LeftTracker.Track();
                        OnLeftDataChange();
                    }
                }
            }
        }

        private Vec2ArrayAnimation _rightAnimation = new Vec2ArrayAnimation(LINE_ANIMATION_LENGTH);
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

                    _rightGraph = value;
                    if (_rightGraph == null)
                    {
                        _hasRightData = false;
                        _lineRenderer.SetHidden(true);
                    }
                    else
                    {
                        _rightGraph = value;
                        _rightGraph.Graph.OnDataChange += OnRightDataChange;
                        RightTracker.TrackedGraph = _rightGraph;
                        RightTracker.Track();
                        OnRightDataChange();
                    }
                }
            }
        }

        private SkinnedMeshLineRenderer _lineRenderer;

        private void OnEnable()
        {
            _lineRenderer = GetComponent<SkinnedMeshLineRenderer>();
            _rightAnimation.Update += SetRightData;
            _leftAnimation.Update += SetLeftData;
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
                UpdateRenderer(UpdateMode.Position);
            }
            else
            {
                var data = _leftGraph.Graph.GetDataPosition();
                if (_hasLeftData && _hasRightData)
                {
                    _leftAnimation.Restart(data);
                }
                else
                {
                    _hasLeftData = true;
                    SetLeftData(data);
                    _leftAnimation.Init(data);
                }
            }

        }
        private void SetLeftData(Vector2[] data)
        {
            Debug.Assert(data.Length == _lineRenderer.Lines.Length);
            for (var i = 0; i < _lineRenderer.Lines.Length; i++)
            {
                _lineRenderer.Lines[i].Start = data[i] + new Vector2(RandomOffsetX[i], RandomOffsetY[i]);
            }
            UpdateRenderer(UpdateMode.Position);
        }

        private void OnRightDataChange()
        {
            if (_rightGraph.Graph.DimX == null || _rightGraph.Graph.DimY == null)
            {
                _hasRightData = false;
                UpdateRenderer(UpdateMode.Position);
            }
            else
            {
                var data = _rightGraph.Graph.GetDataPosition();

                if (_hasLeftData && _hasRightData)
                {
                    _rightAnimation.Restart(data);
                }
                else
                {
                    _hasRightData = true;
                    SetRightData(data);
                    _rightAnimation.Init(data);
                }
            }

        }

        private void SetRightData(Vector2[] data)
        {
            Debug.Assert(data.Length == _lineRenderer.Lines.Length);
            for (var i = 0; i < _lineRenderer.Lines.Length; i++)
            {
                _lineRenderer.Lines[i].End = data[i] + new Vector2(RandomOffsetX[i], RandomOffsetY[i]);
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
