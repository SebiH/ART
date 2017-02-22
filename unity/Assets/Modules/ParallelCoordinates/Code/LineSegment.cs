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
        private float _startTimeDelta = 0f;
        private Vector3 _startDestination;
        public Vector3 DesiredStart
        {
            get { return _startDestination;  }
            set
            {
                _startOrigin = Start;
                _startDestination = value;
                _startTimeDelta = 0f;

                if (!_isStartAnimationRunning)
                {
                    GameLoop.Instance.StartRoutine(RunStartAnimation());
                }
            }
        }

        public Vector3 End = new Vector3(0, 0, 1);

        private bool _isEndAnimationRunning = false;
        private Vector3 _endOrigin;
        private float _endTimeDelta = 0f;
        private Vector3 _endDestination;
        public Vector3 DesiredEnd
        {
            get { return _endDestination; }
            set
            {
                _endOrigin = End;
                _endDestination = value;
                _endTimeDelta = 0f;

                if (!_isEndAnimationRunning)
                {
                    GameLoop.Instance.StartRoutine(RunEndAnimation());
                }
            }
        }

        public Color32 Color = new Color32(25, 118, 210, 255);
        public bool IsFiltered;

        public int MeshIndex = -1;
        // to avoid unnecessary multiple updates in LineRenderer
        public bool WaitingForUpdate = false;

        private GraphicsLineRenderer _renderer;

        public void SetRenderer(GraphicsLineRenderer renderer)
        {
            Debug.Assert(_renderer == null, "Cannot reassign LineSegment to different renderer!");
            _renderer = renderer;
            _renderer.AddLine(this);
        }

        private IEnumerator RunStartAnimation()
        {
            while (_startTimeDelta < 1.0f)
            {
                _startTimeDelta += Time.deltaTime / ANIMATION_SPEED;
                Start = Vector3.Lerp(_startOrigin, _startDestination, _startTimeDelta);
                UpdateVisual();

                yield return new WaitForEndOfFrame();
            }

            _isStartAnimationRunning = false;
        }

        private IEnumerator RunEndAnimation()
        {
            while (_endTimeDelta < 1.0f)
            {
                _endTimeDelta += Time.deltaTime / ANIMATION_SPEED;
                End = Vector3.Lerp(_endOrigin, _endDestination, _endTimeDelta);
                UpdateVisual();

                yield return new WaitForEndOfFrame();
            }

            _isEndAnimationRunning = false;
        }

        public void UpdateVisual()
        {
            if (MeshIndex >= 0 && !WaitingForUpdate)
            {
                _renderer.UpdateLine(this);
            }
        }
    }
}
