using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PointRenderer : MonoBehaviour
    {
        private readonly Mesh _mesh = new Mesh();
        private Graph _graph;
        private MeshFilter _filter;
        private MeshRenderer _renderer;

        public struct PointProperty
        {
            public int DataIndex;
            public Vector2 Position;
            public Color32 Color;
            public float Size;
        }

        public PointProperty[] _points = new PointProperty[Globals.DataPointsCount];

        private void OnEnable()
        {
            var x = new PointProperty();
            Debug.LogError(System.Runtime.InteropServices.Marshal.SizeOf(x));

            _filter = GetComponent<MeshFilter>();
            _filter.mesh = _mesh;

            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;

            _graph = UnityUtility.FindParent<Graph>(this);
            _graph.OnDataChange += GenerateMesh;

            var triangles = new int[_points.Length * 6];

            for (var i = 0; i < _points.Length; i++)
            {
                // triangle indices will stay static
                triangles[i * 6 + 0] = i;
                triangles[i * 6 + 1] = i + 1;
                triangles[i * 6 + 2] = i + 2;
                triangles[i * 6 + 3] = i + 1;
                triangles[i * 6 + 4] = i + 3;
                triangles[i * 6 + 5] = i + 2;

                _points[i] = new PointProperty
                {
                    DataIndex = i,
                    Color = new Color32(255, 255, 255, 255),
                    Size = 0.01f,
                    Position = new Vector2(Random.value - 0.5f, Random.value - 0.5f)
                };
            }

            _mesh.vertices = new Vector3[_points.Length * 4];
            _mesh.colors32 = new Color32[_points.Length * 4];
            _mesh.triangles = triangles;
            _mesh.MarkDynamic();
        }

        private void OnDisable()
        {
            _graph.OnDataChange -= GenerateMesh;
        }

        private void GenerateMesh()
        {
            if (_graph.DimX == null || _graph.DimY == null)
            {
                _renderer.enabled = false;
                return;
            }

            _renderer.enabled = true;

            var vertices = _mesh.vertices;
            var colors = _mesh.colors32;

            for (var i = 0; i < _points.Length; i++)
            {
                var point = _points[i];
                vertices[i * 4 + 0] = new Vector3(point.Position.x - point.Size, point.Position.y + point.Size, 0);
                vertices[i * 4 + 1] = new Vector3(point.Position.x + point.Size, point.Position.y + point.Size, 0);
                vertices[i * 4 + 2] = new Vector3(point.Position.x - point.Size, point.Position.y - point.Size, 0);
                vertices[i * 4 + 3] = new Vector3(point.Position.x + point.Size, point.Position.y - point.Size, 0);

                colors[i * 4 + 0] = point.Color;
                colors[i * 4 + 1] = point.Color;
                colors[i * 4 + 2] = point.Color;
                colors[i * 4 + 3] = point.Color;
            }

            _mesh.vertices = vertices;
            _mesh.colors32 = colors;
            _mesh.RecalculateBounds();
        }
    }
}
