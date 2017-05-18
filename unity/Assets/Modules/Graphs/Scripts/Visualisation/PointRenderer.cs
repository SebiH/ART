using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PointRenderer : MonoBehaviour
    {
        private Mesh _mesh;
        private MeshFilter _filter;
        private MeshRenderer _renderer;

        public struct PointProperty
        {
            public Vector2 Position;
            public Color32 Color;
        }

        public PointProperty[] Points = new PointProperty[0];

        private void OnEnable()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        public void Resize(int length)
        {
            Debug.Assert(Points.Length != length, "Performing unnecessary resize()!");
            Points = new PointProperty[length];
            _mesh = new Mesh();

            _filter = GetComponent<MeshFilter>();
            _filter.mesh = _mesh;

            var triangles = new int[Points.Length * 3];
            var colors = new Color32[Points.Length];
            var uvs = new Vector2[Points.Length];

            for (var i = 0; i < Points.Length; i++)
            {
                // triangles are calculated in geometry shader, only first one triangleindex has to include point
                triangles[i * 3 + 0] = i;
                triangles[i * 3 + 1] = 0;
                triangles[i * 3 + 2] = 1;

                colors[i] = new Color32(255, 255, 255, 255);

                // pass in random z-offset via UV, to avoid z-fighting inbetween points
                uvs[i] = new Vector2(Random.value / 10000f, 0);
            }

            // special cases for first two triangles, cannot have repeating indices in triangle
            triangles[1] = Points.Length - 1;
            triangles[5] = Points.Length - 1;

            _mesh.vertices = new Vector3[Points.Length];
            _mesh.colors32 = colors;
            _mesh.triangles = triangles;
            _mesh.uv2 = uvs;
            _mesh.MarkDynamic();
        }

        public void GenerateMesh()
        {
            if (_mesh == null)
            {
                return;
            }

            var vertices = _mesh.vertices;
            var colors = _mesh.colors32;

            for (var i = 0; i < Points.Length; i++)
            {
                var point = Points[i];
                vertices[i] = new Vector3(point.Position.x, point.Position.y, 0);
                colors[i] = point.Color;
            }

            _mesh.vertices = vertices;
            _mesh.colors32 = colors;
            _mesh.RecalculateBounds();
        }

        public void UpdateColor()
        {
            if (_mesh == null)
            {
                return;
            }

            var colors = _mesh.colors32;

            for (var i = 0; i < Points.Length; i++)
            {
                colors[i] = Points[i].Color;
            }

            _mesh.colors32 = colors;
        }

        public void UpdatePositions()
        {
            if (_mesh == null)
            {
                return;
            }

            var vertices = _mesh.vertices;

            for (var i = 0; i < Points.Length; i++)
            {
                var point = Points[i];
                vertices[i] = new Vector3(point.Position.x, point.Position.y, 0);
            }

            _mesh.vertices = vertices;
            _mesh.RecalculateBounds();
        }

        public void SetHidden(bool isHidden)
        {
            _renderer.enabled = !isHidden;
        }
    }
}
