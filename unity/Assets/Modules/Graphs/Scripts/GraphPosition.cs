using Assets.Modules.Core.Animations;
using System;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(Graph))]
    public class GraphPosition : MonoBehaviour
    {
        // for selection, etc.
        const float NormalAnimationSpeed = 0.6f;
        const float QuickAnimationSpeed = 0.3f;
        // for scrolling, smoothing out values from webapp
        const float FastAnimationSpeed = 0.05f;

        const float OFFSET_SELECTED = 0.2f;
        const float OFFSET_NORMAL = 0.8f;

        const float HEIGHT_NORMAL = 0.5f;
        const float HEIGHT_PICKEDUP = 0.6f;

        private ValueAnimation _positionAnimation = new ValueAnimation(FastAnimationSpeed);
        private ValueAnimation _heightAnimation = new ValueAnimation(QuickAnimationSpeed);
        private ValueAnimation _offsetAnimation = new ValueAnimation(NormalAnimationSpeed);
        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(NormalAnimationSpeed);

        // TODO - animate?
        public float Width { get; set; }

        // Position on table
        private float _position;
        public float Position
        {
            set
            {
                if (value != _position)
                {
                    _position = value;
                    _positionAnimation.Restart(value);
                }
            }

            get
            {
                return _positionAnimation.CurrentValue;
            }
        }

        // Distance from table
        private float _height;
        public float Height
        {
            set
            {
                if (value != _height)
                {
                    _height = value;
                    _heightAnimation.Restart(value);
                }
            }

            get
            {
                return _heightAnimation.CurrentValue;
            }
        }

        // Distance from user
        private float _offset;
        public float Offset
        {
            set
            {
                if (value != _offset)
                {
                    _offset = value;
                    _offsetAnimation.Restart(value);
                }
            }

            get
            {
                return _offsetAnimation.CurrentValue;
            }
        }

        public Vector3 Scale
        {
            set { transform.localScale = value; }
        }

        // x axis rotation
        public float FlipRotation
        {
            get { return _rotationAnimation.CurrentValue.eulerAngles.z; }
        }

        // y axis rotation
        public float SelectRotation
        {
            get { return _rotationAnimation.CurrentValue.eulerAngles.y; }
        }

        private Graph _graph;

        private void OnEnable()
        {
            _graph = GetComponent<Graph>();
        }

        private void Update()
        {
            Offset = _graph.IsSelected ? OFFSET_SELECTED : OFFSET_NORMAL;
            Height = _graph.IsPickedUp ? HEIGHT_PICKEDUP : HEIGHT_NORMAL;

            var rotY = _graph.IsSelected ? 0 : 90;
            var rotZ = _graph.IsFlipped ? 90 : 0;
            var targetRotation = Quaternion.Euler(0, rotY, rotZ);
            if (_rotationAnimation.End != targetRotation)
            {
                _rotationAnimation.Restart(targetRotation);
            }

            var actualPosition = _positionAnimation.CurrentValue;
            var actualHeight = _heightAnimation.CurrentValue;
            var actualOffset = _offsetAnimation.CurrentValue;

            transform.localPosition = new Vector3(actualPosition, actualHeight, actualOffset);
            transform.localRotation = _rotationAnimation.CurrentValue;
        }

        public void Init(float pos, float height, float offset)
        {
            _position = pos;
            _positionAnimation.Init(_position);

            _height = height;
            _heightAnimation.Init(_height);

            _offset = offset;
            _offsetAnimation.Init(_offset);
        }
    }
}
