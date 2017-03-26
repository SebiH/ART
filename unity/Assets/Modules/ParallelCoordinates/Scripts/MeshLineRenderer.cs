using Assets.Modules.Core;
using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class MeshLineRenderer : MonoBehaviour
    {
        private readonly Mesh _mesh = new Mesh();
        private Graph _graph;
        private MeshFilter _filter;
        private MeshRenderer _renderer;

        public struct LineProperty
        {
            public Vector3 Start;
            public Vector3 End;
            public Color32 Color;
            public float Size;
        }

        // Array position == data index
        public LineProperty[] Lines = new LineProperty[Globals.DataPointsCount];

        private void OnEnable()
        {
            var x = new LineProperty();
            Debug.LogError(System.Runtime.InteropServices.Marshal.SizeOf(x));

            _filter = GetComponent<MeshFilter>();
            _filter.mesh = _mesh;

            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;

            _graph = UnityUtility.FindParent<Graph>(this);
            _graph.OnDataChange += GenerateMesh;

            var triangles = new int[Lines.Length * 6];

            for (var i = 0; i < Lines.Length; i++)
            {
                // triangle indices will stay static
                triangles[i * 6 + 0] = i;
                triangles[i * 6 + 1] = i + 1;
                triangles[i * 6 + 2] = i + 2;
                triangles[i * 6 + 3] = i + 1;
                triangles[i * 6 + 4] = i + 3;
                triangles[i * 6 + 5] = i + 2;

                Lines[i] = new LineProperty
                {
                    Start = new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0),
                    End = new Vector3(Random.value - 0.5f, Random.value - 0.5f, 1),
                    Color = new Color32(255, 255, 255, 255),
                    Size = 0.01f
                };
            }

            _mesh.vertices = new Vector3[Lines.Length * 4];
            _mesh.colors32 = new Color32[Lines.Length * 4];
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

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                vertices[i * 4 + 0] = new Vector3(line.Start.x, line.Start.y - line.Size, line.Start.z);
                vertices[i * 4 + 1] = new Vector3(line.End.x, line.End.y + line.Size, line.End.z);
                vertices[i * 4 + 2] = new Vector3(line.Start.x, line.Start.y - line.Size, line.Start.z);
                vertices[i * 4 + 3] = new Vector3(line.End.x, line.End.y - line.Size, line.End.z);

                colors[i * 4 + 0] = line.Color;
                colors[i * 4 + 1] = line.Color;
                colors[i * 4 + 2] = line.Color;
                colors[i * 4 + 3] = line.Color;
            }

            _mesh.vertices = vertices;
            _mesh.colors32 = colors;
            //_mesh.RecalculateBounds();
        }

        public void UpdatePositions()
        {
            var vertices = _mesh.vertices;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                vertices[i * 4 + 0] = new Vector3(line.Start.x, line.Start.y - line.Size, line.Start.z);
                vertices[i * 4 + 1] = new Vector3(line.End.x, line.End.y + line.Size, line.End.z);
                vertices[i * 4 + 2] = new Vector3(line.Start.x, line.Start.y - line.Size, line.Start.z);
                vertices[i * 4 + 3] = new Vector3(line.End.x, line.End.y - line.Size, line.End.z);
            }

            _mesh.vertices = vertices;
        }

        public void UpdateColors()
        {
            var colors = _mesh.colors32;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                colors[i * 4 + 0] = line.Color;
                colors[i * 4 + 1] = line.Color;
                colors[i * 4 + 2] = line.Color;
                colors[i * 4 + 3] = line.Color;
            }

            _mesh.colors32 = colors;
        }
    }
}
