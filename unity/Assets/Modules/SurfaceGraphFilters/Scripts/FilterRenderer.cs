using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters.Scripts
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FilterRenderer : MonoBehaviour
    {
        private MeshFilter _filter;
        private MeshRenderer _renderer;

        private void OnEnable()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
        }


        public void RenderPath(float[][] path)
        {
            var mesh = new Mesh();
            _filter.mesh = mesh;

        }
    }
}
