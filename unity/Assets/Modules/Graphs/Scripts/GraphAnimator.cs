using Assets.Modules.Core.Animations;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(Graph))]
    public class GraphAnimator : MonoBehaviour
    {

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
            var actualPosition = _positionAnimation.CurrentValue;
            var actualHeight = _heightAnimation.CurrentValue;
            var actualOffset = _offsetAnimation.CurrentValue;
            transform.localPosition = new Vector3(actualPosition, actualHeight, actualOffset);
            transform.localRotation = _rotationAnimation.CurrentValue;
            transform.localScale = new Vector3(_graph.Scale, _graph.Scale, 1);
        }
    }
}
