using Assets.Modules.Graphs;
using System;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FilterRenderer : MonoBehaviour
    {
        public int Id { get; set; }

        private MeshFilter _filter;

        private Graph _graph = null;
        private float[] _path = null;
        private Color32 _color = new Color32(255, 255, 255, 255);
        private bool _useGradients = false;
        private GradientStop[] _gradients;

        public struct GradientStop
        {
            public float Stop;
            public Color32 Color;
            public GradientStop(float stop, string color)
            {
                Stop = stop;
                Color = new Color32(255, 255, 255, 255);
                var col = new Color();
                var colorSuccess = ColorUtility.TryParseHtmlString(color, out col);
                if (colorSuccess)
                {
                    Color = col;
                }
            }
        }

        private void OnEnable()
        {
            _filter = GetComponent<MeshFilter>();
            // to avoid z-fighting between filters - needs to be class member for consistent redrawing
            GetComponent<MeshRenderer>().material.SetFloat("_randomOffset", UnityEngine.Random.value / 1000f);
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
            _useGradients = false;
            _color = color;
        }

        public void SetGradient(GradientStop[] gradients)
        {
            _useGradients = true;
            _gradients = gradients.OrderBy(g => g.Stop).ToArray();
        }

        public void UpdateColor(Color32 color)
        {
            SetColor(color);
            RegeneratePath();
        }

        public void UpdateGradient(GradientStop[] gradients)
        {
            SetGradient(gradients);
            RegeneratePath();
        }

        public void RenderPath(float[] path)
        {
            _path = path;

            if (path.Length < 6)
            {
                Debug.Log("Not enough points to render filter: " + path.Length);
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

            try
            {
                if (_useGradients)
                {
                    RenderGradientPath(path);
                }
                else
                {
                    RenderColorPath(path);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void RenderColorPath(float[] path)
        {
            // triangulate polygon into mesh
            var polyVertices = new Vertex[path.Length / 2];
            var dimX = _graph.DimX;
            var dimY = _graph.DimY;

            if (_graph.IsFlipped)
            {
                var temp = dimX;
                dimX = dimY;
                dimY = temp;
            }

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

                var vectors = triangle.vertices;
                if (_graph.IsFlipped)
                {
                    triangles[counter + 0] = counter + 0;
                    triangles[counter + 1] = counter + 1;
                    triangles[counter + 2] = counter + 2;

                    vertices[counter + 0] = new Vector3(Convert.ToSingle(vectors[0].y), Convert.ToSingle(vectors[0].x), 0);
                    vertices[counter + 1] = new Vector3(Convert.ToSingle(vectors[1].y), Convert.ToSingle(vectors[1].x), 0);
                    vertices[counter + 2] = new Vector3(Convert.ToSingle(vectors[2].y), Convert.ToSingle(vectors[2].x), 0);
                }
                else
                {
                    triangles[counter + 0] = counter + 0;
                    triangles[counter + 1] = counter + 2;
                    triangles[counter + 2] = counter + 1;

                    vertices[counter + 0] = new Vector3(Convert.ToSingle(vectors[0].x), Convert.ToSingle(vectors[0].y), 0);
                    vertices[counter + 1] = new Vector3(Convert.ToSingle(vectors[1].x), Convert.ToSingle(vectors[1].y), 0);
                    vertices[counter + 2] = new Vector3(Convert.ToSingle(vectors[2].x), Convert.ToSingle(vectors[2].y), 0);
                }

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
        
        private void RenderGradientPath(float[] path)
        {
            // triangulate polygon into mesh
            var polyVertices = new Vertex[path.Length / 2];
            var dimX = _graph.DimX;
            var dimY = _graph.DimY;

            if (_graph.IsFlipped)
            {
                var temp = dimX;
                dimX = dimY;
                dimY = temp;
            }

            for (var i = 0; i < path.Length / 2; i++)
            {
                polyVertices[i] = new Vertex(dimX.Scale(path[i * 2]), dimY.Scale(path[i * 2 + 1]));
            }

            var polygon = new Polygon();
            polygon.Add(new Contour(polyVertices));

            var options = new ConstraintOptions { Convex = false, ConformingDelaunay = false };
            var quality = new QualityOptions { };
            var generatedMesh = polygon.Triangulate(options, quality);

            // quick hack: gradient cannot expand outside of graph bounds [-0.5, 0.5]
            double min, max;
            //if (_graph.IsFlipped)
            //{
            //    min = Math.Max(generatedMesh.Bounds.Top, -0.5);
            //    max = Math.Min(generatedMesh.Bounds.Bottom, 0.5);
            //}
            //else
            //{
                min = Math.Max(generatedMesh.Bounds.Left, -0.5);
                max = Math.Min(generatedMesh.Bounds.Right, 0.5);
            //}
            var range = max - min;

            // convert triangulated mesh into unity mesh
            var triangles = new int[generatedMesh.Triangles.Count * 3];
            var vertices = new Vector3[generatedMesh.Triangles.Count * 3];
            var colors = new Color32[vertices.Length];
            var counter = 0;

            foreach (var triangle in generatedMesh.Triangles)
            {
                var vectors = triangle.vertices;

                if (_graph.IsFlipped)
                {
                    triangles[counter + 0] = counter + 0;
                    triangles[counter + 1] = counter + 1;
                    triangles[counter + 2] = counter + 2;

                    vertices[counter + 0] = new Vector3(Convert.ToSingle(vectors[0].y), Convert.ToSingle(vectors[0].x), 0);
                    vertices[counter + 1] = new Vector3(Convert.ToSingle(vectors[1].y), Convert.ToSingle(vectors[1].x), 0);
                    vertices[counter + 2] = new Vector3(Convert.ToSingle(vectors[2].y), Convert.ToSingle(vectors[2].x), 0);

                    colors[counter + 0] = GetGradient((vectors[0].x - min) / range);
                    colors[counter + 1] = GetGradient((vectors[1].x - min) / range);
                    colors[counter + 2] = GetGradient((vectors[2].x - min) / range);
                }
                else
                {
                    triangles[counter + 0] = counter + 0;
                    triangles[counter + 1] = counter + 2;
                    triangles[counter + 2] = counter + 1;

                    vertices[counter + 0] = new Vector3(Convert.ToSingle(vectors[0].x), Convert.ToSingle(vectors[0].y), 0);
                    vertices[counter + 1] = new Vector3(Convert.ToSingle(vectors[1].x), Convert.ToSingle(vectors[1].y), 0);
                    vertices[counter + 2] = new Vector3(Convert.ToSingle(vectors[2].x), Convert.ToSingle(vectors[2].y), 0);

                    colors[counter + 0] = GetGradient((vectors[0].x - min) / range);
                    colors[counter + 1] = GetGradient((vectors[1].x - min) / range);
                    colors[counter + 2] = GetGradient((vectors[2].x - min) / range);
                }

                counter += 3;
            }

            var mesh = new Mesh();
            _filter.mesh = mesh;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors32 = colors;
            //mesh.RecalculateBounds();
        }

        private Color32 GetGradient(double position)
        {
            if (position <= 0)
            {
                return _gradients[0].Color;
            }

            if (position >= 1)
            {
                return _gradients[_gradients.Length - 1].Color;
            }


            for (var i = 0; i < _gradients.Length - 1; i++)
            {
                var g1 = _gradients[i];
                var g2 = _gradients[i + 1];

                if (g1.Stop <= position && position <= g2.Stop)
                {
                    var weight = (position - g1.Stop) / (g2.Stop - g1.Stop);
                    return Color32.Lerp(g1.Color, g2.Color, Convert.ToSingle(weight));
                }
            }

            return new Color32(255, 255, 255, 255);
        }
    }
}
