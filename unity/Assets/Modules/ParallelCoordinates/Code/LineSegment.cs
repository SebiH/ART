using Assets.Modules.Core.Animations;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineSegment
    {
        const float ANIMATION_SPEED = 1f; // in seconds

        private VectorAnimation _startAnimation = new VectorAnimation(ANIMATION_SPEED);
        private VectorAnimation _endAnimation = new VectorAnimation(ANIMATION_SPEED);

        private Vector3 _start;
        public Vector3 Start
        {
            get { return _start; }
            set
            {
                if (_start == value) { return; }
                _start = value;
                _startAnimation.Stop();
                UpdatePosition();
            }
        }

        public Vector3 AnimatedStart
        {
            set
            {
                if (_start == value) { return; }
                _startAnimation.Restart(_start, value);
                _start = value;
            }
        }

        private Vector3 _end;
        public Vector3 End
        {
            get { return _end; }
            set
            {
                if (_end == value) { return; }
                _end = value;
                _endAnimation.Stop();
                UpdatePosition();
            }
        }

        public Vector3 AnimatedEnd
        {
            set
            {
                if (_end == value) { return; }
                _endAnimation.Restart(_end, value);
                _end = value;
            }
        }


        private Color32 _color = new Color32(255, 255, 255, 255);
        public Color32 Color
        {
            get { return _color; }
            set
            {
                if (_color.r != value.r || _color.g != value.g || _color.b != value.b)
                {
                    _color.r = value.r;
                    _color.g = value.g;
                    _color.b = value.b;
                    UpdateColor();
                }
            }
        }

        public byte Transparency
        {
            get { return _color.a; }
            set
            {
                if (_color.a != value)
                {
                    _color.a = value;
                    UpdateColor();
                }
            }
        }


        public int MeshIndex = -1;

        // to avoid unnecessary multiple updates in LineRenderer
        public bool WaitingForVertex = false;
        public bool WaitingForColor = false;

        private GraphicsLineRenderer _renderer;


        public LineSegment()
        {
            _startAnimation.Update += (val) => UpdatePosition();
            _endAnimation.Update += (val) => UpdatePosition();
        }


        public void SetRenderer(GraphicsLineRenderer renderer)
        {
            _renderer = renderer;
            _renderer.AddLine(this);
        }

        private void UpdateColor()
        {
            if (MeshIndex >= 0 && !WaitingForColor)
            {
                _renderer.UpdateLineColor(this);
            }
        }

        private void UpdatePosition()
        {
            if (MeshIndex >= 0 && !WaitingForVertex)
            {
                _renderer.UpdateLineVertices(this);
            }
        }
    }
}
