using Assets.Modules.Core;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineSegment
    {
        const float ANIMATION_SPEED = 1.5f; // in seconds

        public Vector3 Start;

        private bool _isStartAnimationRunning = false;
        private Vector3 _startOrigin;
        private float _startTime;
        private Vector3 _startDestination;
        public Vector3 DesiredStart
        {
            get { return _startDestination;  }
            set
            {
                _startOrigin = Start;
                _startDestination = value;
                _startTime = Time.time;

                if (!_isStartAnimationRunning)
                {
                    GameLoop.Instance.StartRoutine(RunStartAnimation());
                }
            }
        }

        public Vector3 End = new Vector3(0, 0, 1);

        private bool _isEndAnimationRunning = false;
        private Vector3 _endOrigin;
        private float _endTime;
        private Vector3 _endDestination;
        public Vector3 DesiredEnd
        {
            get { return _endDestination; }
            set
            {
                _endOrigin = End;
                _endDestination = value;
                _endTime = Time.time;

                if (!_isEndAnimationRunning)
                {
                    GameLoop.Instance.StartRoutine(RunEndAnimation());
                }
            }
        }


        public Color32 Color = new Color32(25, 118, 210, 255);

        private bool _isColorAnimationRunning = false;
        private Color32 _colorOrigin;
        private float _colorTime;
        private Color32 _colorDestination;
        public Color32 DesiredColor
        {
            get { return _colorDestination; }
            set
            {
                _colorOrigin = Color;
                _colorDestination = value;
                _colorTime = Time.time;

                if (!_isColorAnimationRunning)
                {
                    GameLoop.Instance.StartRoutine(RunColorAnimation());
                }
            }
        }



        private bool _isFiltered;
        public bool IsFiltered
        {
            get { return _isFiltered; }
            set { _isFiltered = value;  UpdatePosition(); }
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
                Start = Vector3.Lerp(_startOrigin, _startDestination, timeDelta);
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
                End = Vector3.Lerp(_endOrigin, _endDestination, timeDelta);
                UpdatePosition();

                yield return new WaitForEndOfFrame();
            }

            _isEndAnimationRunning = false;
        }

        private IEnumerator RunColorAnimation()
        {
            var timeDelta = 0f;
            while (timeDelta < 1.0f)
            {
                timeDelta = (Time.time - _colorTime) / ANIMATION_SPEED;
                Color = Color32.Lerp(_colorOrigin, _colorDestination, timeDelta);
                UpdateColor();

                yield return new WaitForEndOfFrame();
            }

            _isColorAnimationRunning = false;
        }

        public void UpdateColor()
        {
            if (MeshIndex >= 0 && !WaitingForColor)
            {
                _renderer.UpdateLineColor(this);
            }
        }

        public void UpdatePosition()
        {
            if (MeshIndex >= 0 && !WaitingForVertex)
            {
                _renderer.UpdateLineVertices(this);
            }
        }
    }
}
