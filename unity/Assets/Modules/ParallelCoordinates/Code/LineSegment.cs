using Assets.Modules.Core;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineSegment
    {
        const float ANIMATION_SPEED = 1f; // in seconds

        private Vector3 _start;
        public Vector3 Start
        {
            get { return _start; }
            set
            {
                if (_start == value) { return; }

                _start = value;
                _startOrigin = value;
                _startDestination = value;
                _startTime = 0f; // stops animation the next time it's running

                UpdatePosition();
            }
        }

        private bool _isStartAnimationRunning = false;
        private Vector3 _startOrigin;
        private float _startTime;
        private Vector3 _startDestination;
        public Vector3 DesiredStart
        {
            get { return _startDestination;  }
            set
            {
                if (_startDestination == value) { return; }

                // don't animate z-values due to conflict with z-scaling in GraphConnection
                _startOrigin = new Vector3(_start.x, Start.y, value.z);
                _start = _startOrigin;
                _startDestination = value;
                _startTime = Time.time;

                // set z immediately to avoid scaling issues..
                _start = new Vector3(_start.x, _start.y, value.z);

                if (!_isStartAnimationRunning)
                {
                    GameLoop.Instance.StartRoutine(RunStartAnimation());
                }
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
                _endOrigin = value;
                _endDestination = value;
                _endTime = 0f; // stops animation the next time it's running

                UpdatePosition();
            }
        }

        private bool _isEndAnimationRunning = false;
        private Vector3 _endOrigin;
        private float _endTime;
        private Vector3 _endDestination;
        public Vector3 DesiredEnd
        {
            get { return _endDestination; }
            set
            {
                if (_endDestination == value) { return; }

                // don't animate z-values due to conflict with z-scaling in GraphConnection
                _endOrigin = new Vector3(_end.x, _end.y, value.z);
                _end = _endOrigin;
                _endDestination = value;
                _endTime = Time.time;


                if (!_isEndAnimationRunning)
                {
                    GameLoop.Instance.StartRoutine(RunEndAnimation());
                }
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

        public void SetRenderer(GraphicsLineRenderer renderer)
        {
            _renderer = renderer;
            _renderer.AddLine(this);
        }

        private IEnumerator RunStartAnimation()
        {
            var timeDelta = 0f;
            while (timeDelta < 1.0f)
            {
                timeDelta = (Time.time - _startTime) / ANIMATION_SPEED;
                _start = Vector3.Lerp(_startOrigin, _startDestination, timeDelta);
                UpdatePosition();

                yield return new WaitForEndOfFrame();
            }

            _isStartAnimationRunning = false;
        }

        private IEnumerator RunEndAnimation()
        {
            var timeDelta = 0f;
            while (timeDelta < 1.0f)
            {
                timeDelta = (Time.time - _endTime) / ANIMATION_SPEED;
                _end = Vector3.Lerp(_endOrigin, _endDestination, timeDelta);
                UpdatePosition();

                yield return new WaitForEndOfFrame();
            }

            _isEndAnimationRunning = false;
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
