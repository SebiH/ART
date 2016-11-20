using UnityEngine;
using System.Collections;

namespace Assets.Modules.Graph
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



        public float TargetHeight
        {
            set
            {
                StopCoroutine("AnimatePosition");
                StartCoroutine("AnimatePosition", value);
            }
        }

        private IEnumerator AnimatePosition(float targetHeight)
        {
            var currentVelocity = 0f;
            while (Mathf.Abs(Height - targetHeight) > Mathf.Epsilon)
            {
                Height = Mathf.SmoothDamp(Height, targetHeight, ref currentVelocity, 0.25f);
                // resume after next update
                yield return null;
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
    }
}
