using UnityEngine;

namespace Assets.Code.Graph
{
    public abstract class DataPoint : MonoBehaviour
    {
        protected bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnHighlightChange(value);
            }
        }



        protected Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                var renderer = GetComponent<MeshRenderer>();
                renderer.material.color = value;
                _color = value;
            }
        }



        protected float _height;
        public float Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
                OnHeightChange(value);
            }
        }



        protected float _targetHeight;
        public float TargetHeight
        {
            get
            {
                return _targetHeight;
            }

            set
            {
                _targetHeight = value;

                float animationTime = 0f;

                // separate initialisation since handler removes itself after it has finished
                OnUpdateHandler animateHeight = null;
                animateHeight = () =>
                {
                    animationTime += Time.deltaTime;
                    var interpolatedHeight = Mathf.Lerp(Height, _targetHeight, animationTime);
                    Height = interpolatedHeight;

                    // stop animating once we have reached desired height
                    if (Mathf.Abs(_targetHeight - Height) < Mathf.Epsilon)
                    {
                        // animation finished, unregister handler
                        OnUpdate -= animateHeight;
                    }
                };

                OnUpdate += animateHeight;

            }
        }


        public void SetPosition(float x, float y)
        {
            var currPos = transform.localPosition;
            transform.localPosition = new Vector3(x, currPos.y, y);
        }


        /*
         *  Abstract methods
         */
        abstract protected void OnHeightChange(float height);
        abstract protected void OnHighlightChange(bool isHighlighted);



        /*
         *  Unity events
         */
        private delegate void OnUpdateHandler();
        private event OnUpdateHandler OnUpdate;

        void Update()
        {
            // TODO: replace with static utility updater?
            if (OnUpdate != null)
            {
                OnUpdate();
            }
        }
    }
}
