using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    [RequireComponent(typeof(Graph))]
    public class GraphAnimator : MonoBehaviour
    {
        // for selection, etc.
        public float NormalAnimationSpeed = 0.6f;
        // for scrolling, smoothing out values from webapp
        public float FastAnimationSpeed = 0.05f;

        private ValueAnimation _positionAnimation = new ValueAnimation(0.05f);
        private ValueAnimation _heightAnimation = new ValueAnimation(0.4f);
        private ValueAnimation _offsetAnimation = new ValueAnimation(0.4f);
        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(0.4f);

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
        }

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
        }

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
        }

        private Quaternion _rotation;
        public Quaternion Rotation {
            set
            {
                if (value != _rotation)
                {
                    _rotation = value;
                    _rotationAnimation.Restart(value);
                }
            }
        }

        private Graph _graph;

        private void OnEnable()
        {
            _graph = GetComponent<Graph>();

            transform.localRotation = Quaternion.Euler(0, 90, 0);
            transform.localPosition = new Vector3(_graph.Position, -0.5f, 0.5f);
            _graph.Scale = 0.55f;

            _position = transform.localPosition.x;
            _positionAnimation.Init(_position);

            _height = transform.localPosition.y;
            _heightAnimation.Init(_height);

            _offset = transform.localPosition.z;
            _offsetAnimation.Init(_offset);

            _rotation = transform.localRotation;
            _rotationAnimation.Init(_rotation);
        }

        private void Update()
        {
            _positionAnimation.AnimationSpeed = FastAnimationSpeed;
            _heightAnimation.AnimationSpeed = NormalAnimationSpeed;
            _offsetAnimation.AnimationSpeed = NormalAnimationSpeed;
            _rotationAnimation.AnimationSpeed = NormalAnimationSpeed;

            var actualPosition = _positionAnimation.CurrentValue;
            var actualHeight = _heightAnimation.CurrentValue;
            var actualOffset = _offsetAnimation.CurrentValue;
            transform.localPosition = new Vector3(actualPosition, actualHeight, actualOffset);
            transform.localRotation = _rotationAnimation.CurrentValue;
            transform.localScale = new Vector3(_graph.Scale, _graph.Scale, 1);
        }
    }
}
