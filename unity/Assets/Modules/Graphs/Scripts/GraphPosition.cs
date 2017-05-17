using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(Graph))]
    public class GraphPosition : MonoBehaviour
    {
        public float OffsetSelected= -0.4f;
        public float OffsetNormal = 0;

        public float HeightNormal = 0.33f;
        public float HeightPickedUp = 0.5f;

        public float Scale = 0.6f;

        private ValueAnimation _positionAnimation = new ValueAnimation(Globals.FastAnimationSpeed);
        private ValueAnimation _heightAnimation = new ValueAnimation(Globals.QuickAnimationSpeed);
        private ValueAnimation _offsetAnimation = new ValueAnimation(Globals.NormalAnimationSpeed);
        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(Globals.NormalAnimationSpeed);

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

        private void LateUpdate()
        {
            Offset = _graph.IsSelected ? OffsetSelected : OffsetNormal;
            Height = _graph.IsPickedUp ? HeightPickedUp : HeightNormal;

            var rotZ = _graph.IsFlipped ? 90 : 0;

            var rotY = 0;
            if (_graph.IsFlipped)
                rotY = _graph.IsSelected ? 180 : -90;
            else
                rotY = _graph.IsSelected ? 0 : 90;


            var targetRotation = Quaternion.Euler(0, rotY, rotZ);
            if (_rotationAnimation.End != targetRotation)
            {
                _rotationAnimation.Restart(targetRotation);
            }

            var actualPosition = _position; //_positionAnimation.CurrentValue;
            var actualHeight = _heightAnimation.CurrentValue;
            var actualOffset = _offsetAnimation.CurrentValue;

            transform.localPosition = new Vector3(actualPosition, actualHeight, actualOffset);
            transform.localRotation = _rotationAnimation.CurrentValue;
            transform.localScale = Vector3.one * Scale;
        }

        public void Init(float pos, float height, float offset)
        {
            _position = pos;
            _positionAnimation.Init(_position);

            _height = height;
            _heightAnimation.Init(_height);

            _offset = offset;
            _offsetAnimation.Init(_offset);

            var rotation = Quaternion.Euler(0, 90, 0);
            _rotationAnimation.Init(rotation);

            transform.localPosition = new Vector3(_position, _height, _offset);
            transform.localRotation = rotation;
        }
    }
}
