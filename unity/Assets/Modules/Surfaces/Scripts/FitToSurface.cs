using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class FitToSurface : MonoBehaviour
    {
        public Vector3 Offset = Vector3.zero;
        private Surface _surface;

        private void Start()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
        }

        private void Update()
        {
            transform.localScale = new Vector3(_surface.Scale.x, transform.localScale.y, _surface.Scale.z);
            transform.localPosition = _surface.Scale / 2 + Offset;
        }
    }
}
