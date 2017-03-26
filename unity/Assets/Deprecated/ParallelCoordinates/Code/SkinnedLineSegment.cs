using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates_Deprecated
{
    public class SkinnedLineSegment
    {
        public GameObject go;
        private static int counter = 0;
        public SkinnedLineSegment(Transform parent)
        {
            go = new GameObject();
            go.name = "Line" + counter++;
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }

        const float ANIMATION_SPEED = 1.5f; // in seconds

        private Vector3 _start;
        public Vector3 Start
        {
            get { return _start; }
            set { _start = value; UpdatePosition(); }
        }

        private bool _isStartAnimationRunning = false;
        private Vector3 _startOrigin;
        private float _startTimeDelta = 0f;
        private Vector3 _startDestination;
        public Vector3 DesiredStart
        {
            get { return _startDestination; }
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

        private Vector3 _end;
        public Vector3 End
        {
            get { return _end; }
            set { _end = value; UpdatePosition(); }
        }

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

        private bool _isColorAnimationRunning = false;
        private Color32 _colorOrigin;
        private float _colorTimeDelta = 0f;
        private Color32 _colorDestination;
        public Color32 DesiredColor
        {
            get { return _colorDestination; }
            set
            {
                _colorOrigin = Color;
                _colorDestination = value;
                _colorTimeDelta = 0f;

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
            set { _isFiltered = value; UpdatePosition(); }
        }


        public int MeshIndex = -1;

        // to avoid unnecessary multiple updates in LineRenderer
        public bool WaitingForVertex = false;
        public bool WaitingForColor = false;

        private SkinnedLineRenderer _renderer;

        public void SetRenderer(SkinnedLineRenderer renderer)
        {
            _renderer = renderer;
            _renderer.AddLine(this);
        }

        private IEnumerator RunStartAnimation()
        {
            while (_startTimeDelta < 1.0f)
            {
                _startTimeDelta += Time.deltaTime / ANIMATION_SPEED;
                Start = Vector3.Lerp(_startOrigin, _startDestination, _startTimeDelta);
                UpdatePosition();

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
                UpdatePosition();

                yield return new WaitForEndOfFrame();
            }

            _isEndAnimationRunning = false;
        }

        private IEnumerator RunColorAnimation()
        {
            while (_colorTimeDelta < 1.0f)
            {
                _colorTimeDelta += Time.deltaTime / ANIMATION_SPEED;
                Color = Color32.Lerp(_colorOrigin, _colorDestination, _colorTimeDelta);
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
            go.transform.localPosition = new Vector3(-Start.x, Start.y, Start.z);
            go.transform.LookAt(new Vector3(-End.x, End.y, End.z), Vector3.up);
        }
    }
}
