using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FilterRenderer : MonoBehaviour
    {
        public int Id;

        private MeshFilter _filter;
        private MeshRenderer _renderer;

        private void OnEnable()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
        }


        public void RenderPath(float[] path)
        {
            var mesh = new Mesh();
            _filter.mesh = mesh;

            var polygon = new Polygon(path.Length);
            Debug.Assert(path.Length % 2 == 0, "Expected pairs in path");
            for (var i = 0; i < path.Length; i += 2)
            {
                polygon.Add(new Vertex(path[i], path[i + 1]));
            }

            var options = new ConstraintOptions { Convex = false, ConformingDelaunay = true };
            var quality = new QualityOptions { };
            var generatedMesh = polygon.Triangulate(options, quality);

        }
    }
}
