using Assets.Modules.Core.Animations;
using System;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(Graph))]
    public class GraphPosition : MonoBehaviour
    {
        // for selection, etc.
        public float NormalAnimationSpeed = 0.6f;
        // for scrolling, smoothing out values from webapp
        public float FastAnimationSpeed = 0.05f;

        private ValueAnimation _positionAnimation = new ValueAnimation(0.05f);
        private ValueAnimation _heightAnimation = new ValueAnimation(0.6f);
        private ValueAnimation _offsetAnimation = new ValueAnimation(0.6f);
        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(0.6f);


        // Position on table
        private float _position;
        public float Position {
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
        public float Height {
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
        public float Offset {
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
        public event Action PositionUpdate;

        private void OnEnable()
        {
            _graph = GetComponent<Graph>();

            _position = transform.localPosition.x;
            _positionAnimation.Init(_position);

            _height = transform.localPosition.y;
            _heightAnimation.Init(_height);

            _offset = transform.localPosition.z;
            _offsetAnimation.Init(_offset);

            _rotationAnimation.Init(transform.localRotation);
        }


        private void Update()
        {
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

        public bool IsParallelTo(GraphPosition other)
        {
            return false;
        }
    }
}
