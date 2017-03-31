using Assets.Modules.Graphs;
using System;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FilterRenderer : MonoBehaviour
    {
        const byte TRANSPARENCY = 80;

        public int Id { get; set; }

        private MeshFilter _filter;
        private MeshRenderer _renderer;

        private Graph _graph = null;
        private float[] _path = null;
        private Color32 _color = new Color32(255, 255, 255, TRANSPARENCY);

        private void OnEnable()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
        }

        private void OnDisable()
        {
            if (_graph)
            {
                _graph.OnDataChange -= RegeneratePath;
            }
        }

        public void Init(GraphMetaData g)
        {
            transform.SetParent(g.Visualisation.transform, false);
            _graph = g.Graph;
            _graph.OnDataChange += RegeneratePath;
        }

        private void RegeneratePath()
        {
            if (_path != null)
            {
                RenderPath(_path);
            }
        }

        public void SetColor(Color32 color)
        {
            _color = new Color32(color.r, color.g, color.b, TRANSPARENCY);
        }

        public void UpdateColor(Color32 color)
        {
            SetColor(color);
            RegeneratePath();
        }

        public void RenderPath(float[] path)
        {
            _path = path;

            if (path.Length < 6)
            {
                Debug.Log("Not enough points to render filter");
                return;
            }

            if (path.Length % 2 != 0)
            {
                Debug.LogError("Expected pairs in path");
                return;
            }

            if (_graph.DimX == null || _graph.DimY == null)
            {
                // might occur if dimensions haven't been loaded yet
                Debug.LogWarning("Graph has filters without dimensions");
                return;
            }


            // triangulate polygon into mesh
            var polyVertices = new Vertex[path.Length / 2];
            var dimX = _graph.DimX;
            var dimY = _graph.DimY;
            for (var i = 0; i < path.Length / 2; i++)
            {
                polyVertices[i] = new Vertex(dimX.Scale(path[i * 2]), dimY.Scale(path[i * 2 + 1]));
            }

            var polygon = new Polygon();
            polygon.Add(new Contour(polyVertices));

            var options = new ConstraintOptions { Convex = false, ConformingDelaunay = false };
            var quality = new QualityOptions { };
            var generatedMesh = polygon.Triangulate(options, quality);


            // convert triangulated mesh into unity mesh
            var triangles = new int[generatedMesh.Triangles.Count * 3];
            var vertices = new Vector3[generatedMesh.Triangles.Count * 3];
            var colors = new Color32[vertices.Length];
            var counter = 0;

            foreach (var triangle in generatedMesh.Triangles)
            {
                triangles[counter + 0] = counter + 0;
                triangles[counter + 1] = counter + 2;
                triangles[counter + 2] = counter + 1;

                var vectors = triangle.vertices;
                vertices[counter + 0] = new Vector3(Convert.ToSingle(vectors[0].x), Convert.ToSingle(vectors[0].y), 0);
                vertices[counter + 1] = new Vector3(Convert.ToSingle(vectors[1].x), Convert.ToSingle(vectors[1].y), 0);
                vertices[counter + 2] = new Vector3(Convert.ToSingle(vectors[2].x), Convert.ToSingle(vectors[2].y), 0);

                colors[counter + 0] = _color;
                colors[counter + 1] = _color;
                colors[counter + 2] = _color;

                counter += 3;
            }

            var mesh = new Mesh();
            _filter.mesh = mesh;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors32 = colors;
            //mesh.RecalculateBounds();
        }
    }
}
