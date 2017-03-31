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
            public float Size;
        }

        public PointProperty[] Points = new PointProperty[Globals.DataPointsCount];

        private void OnEnable()
        {
            _mesh = new Mesh();

            _filter = GetComponent<MeshFilter>();
            _filter.mesh = _mesh;

            _renderer = GetComponent<MeshRenderer>();

            var triangles = new int[Points.Length * 6];
            var colors = new Color32[Points.Length * 4];

            for (var i = 0; i < Points.Length; i++)
            {
                // triangle indices will stay static
                triangles[i * 6 + 0] = i * 4 + 0;
                triangles[i * 6 + 1] = i * 4 + 1;
                triangles[i * 6 + 2] = i * 4 + 2;
                triangles[i * 6 + 3] = i * 4 + 1;
                triangles[i * 6 + 4] = i * 4 + 3;
                triangles[i * 6 + 5] = i * 4 + 2;

                colors[i * 4 + 0] = new Color32(255, 255, 255, 255);
                colors[i * 4 + 1] = new Color32(255, 255, 255, 255);
                colors[i * 4 + 2] = new Color32(255, 255, 255, 255);
                colors[i * 4 + 3] = new Color32(255, 255, 255, 255);

                Points[i] = new PointProperty
                {
                    Color = new Color32(255, 255, 255, 255),
                    Size = 0.005f,
                    Position = Vector2.zero
                };
            }

            _mesh.vertices = new Vector3[Points.Length * 4];
            _mesh.colors32 = colors;
            _mesh.triangles = triangles;
            _mesh.MarkDynamic();
        }

        public void GenerateMesh()
        {
            var vertices = _mesh.vertices;
            var colors = _mesh.colors32;

            for (var i = 0; i < Points.Length; i++)
            {
                var point = Points[i];
                var rndOffset = (Random.value - 0.5f) / 1000f;
                vertices[i * 4 + 0] = new Vector3(point.Position.x - point.Size, point.Position.y + point.Size, rndOffset);
                vertices[i * 4 + 1] = new Vector3(point.Position.x + point.Size, point.Position.y + point.Size, rndOffset);
                vertices[i * 4 + 2] = new Vector3(point.Position.x - point.Size, point.Position.y - point.Size, rndOffset);
                vertices[i * 4 + 3] = new Vector3(point.Position.x + point.Size, point.Position.y - point.Size, rndOffset);

                colors[i * 4 + 0] = point.Color;
                colors[i * 4 + 1] = point.Color;
                colors[i * 4 + 2] = point.Color;
                colors[i * 4 + 3] = point.Color;
            }

            _mesh.vertices = vertices;
            _mesh.colors32 = colors;
            //_mesh.RecalculateBounds();
        }

        public void UpdateColor()
        {
            var colors = _mesh.colors32;

            for (var i = 0; i < Points.Length; i++)
            {
                var point = Points[i];
                colors[i * 4 + 0] = point.Color;
                colors[i * 4 + 1] = point.Color;
                colors[i * 4 + 2] = point.Color;
                colors[i * 4 + 3] = point.Color;
            }

            _mesh.colors32 = colors;
        }

        public void UpdatePositions()
        {
            var vertices = _mesh.vertices;

            for (var i = 0; i < Points.Length; i++)
            {
                var point = Points[i];
                // to avoid z-fighting
                var rndOffset = (Random.value - 0.5f) / 1000f;
                vertices[i * 4 + 0] = new Vector3(point.Position.x - point.Size, point.Position.y + point.Size, rndOffset);
                vertices[i * 4 + 1] = new Vector3(point.Position.x + point.Size, point.Position.y + point.Size, rndOffset);
                vertices[i * 4 + 2] = new Vector3(point.Position.x - point.Size, point.Position.y - point.Size, rndOffset);
                vertices[i * 4 + 3] = new Vector3(point.Position.x + point.Size, point.Position.y - point.Size, rndOffset);
            }

            _mesh.vertices = vertices;
            //_mesh.RecalculateBounds();
        }

        public void SetHidden(bool isHidden)
        {
            _renderer.enabled = !isHidden;
        }
    }
}
