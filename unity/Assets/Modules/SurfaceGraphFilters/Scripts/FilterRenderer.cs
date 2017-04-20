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
        public int Id { get; set; }

        private Color32 _color;
        public Color32 Color
        {
            get { return _color; }
            set
            {
                if (_color.r != value.r || _color.g != value.r || _color.b != value.b || _color.a != value.a)
                {
                    _useGradient = false;
                    _color = value;
                    _needsUpdate = true;
                }
            }
        }

        public char GradientAxis = 'x'; // 'x' | 'y'
        private GradientStop[] _gradients = null;
        public GradientStop[] Gradients
        {
            get { return _gradients; }
            set
            {
                _useGradient = true;
                _gradients = value;
                _needsUpdate = true;
            }
        }

        private float _minGradient = 0;
        public float MinGradient
        {
            get { return _minGradient; }
            set
            {
                if (_minGradient != value)
                {
                    _minGradient = value;
                    _needsUpdate = true;
                }
            }
        }


        private float _maxGradient = 1;
        public float MaxGradient
        {
            get { return _maxGradient; }
            set
            {
                if (_maxGradient != value)
                {
                    _maxGradient = value;
                    _needsUpdate = true;
                }
            }
        }


        private float[] _path = null;
        public float[] Path
        {
            get { return _path; }
            set
            {
                // try to reduce drawcalls - paths below 6 vertices (12 x&y floats) do not change
                if (_path == null || _path.Length != value.Length || value.Length < 12)
                {
                    _path = value;
                    _needsUpdate = true;
                }
            }
        }
        


        private MeshFilter _filter;
        private Graph _graph;

        private bool _useGradient = false;
        private bool _needsUpdate;


        private void OnEnable()
        {
            _filter = GetComponent<MeshFilter>();
            GetComponent<MeshRenderer>().material.SetFloat("_randomOffset", UnityEngine.Random.value / 1000f + 0.0001f);
        }

        public void Init(GraphMetaData gm)
        {
            transform.SetParent(gm.Visualisation.transform, false);
            _graph = gm.Graph;
        }


        private void LateUpdate()
        {
            if (_needsUpdate && _graph != null && _graph.DimX != null && _graph.DimY != null)
            {
                _needsUpdate = false;

                if (_path == null || _path.Length < 6)
                {
                    Debug.Log("Not enough points to render filter");
                    return;
                }

                if (_path.Length % 2 != 0)
                {
                    Debug.LogError("Expected pairs in path");
                    return;
                }

                try
                {
                    if (_useGradient)
                    {
                        RenderGradientPath(_path);
                    }
                    else
                    {
                        RenderColorPath(_path);

                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }


        private void RenderColorPath(float[] path)
        {
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
                var vectors = triangle.vertices;
                triangles[counter + 0] = counter + 0;
                triangles[counter + 1] = counter + 2;
                triangles[counter + 2] = counter + 1;

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
            mesh.RecalculateBounds();
        }



        private void RenderGradientPath(float[] path)
        {
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
            // restrict maximumarea to allow proper vertex-colouring of gradients
            var quality = new QualityOptions { MaximumArea = 0.005 };
            var generatedMesh = polygon.Triangulate(options, quality);

            // quick hack: gradient cannot expand outside of graph bounds [-0.5, 0.5]
            float min, max;
            if (GradientAxis == 'x')
            {
                min = dimX.Scale(MinGradient);
                max = dimX.Scale(MaxGradient);
            }
            else
            {
                min = dimY.Scale(MinGradient);
                max = dimY.Scale(MaxGradient);
            }

            var range = max - min;

            // convert triangulated mesh into unity mesh
            var triangles = new int[generatedMesh.Triangles.Count * 3];
            var vertices = new Vector3[generatedMesh.Triangles.Count * 3];
            var colors = new Color32[vertices.Length];
            var counter = 0;

            foreach (var triangle in generatedMesh.Triangles)
            {
                var vectors = triangle.vertices;

                triangles[counter + 0] = counter + 0;
                triangles[counter + 1] = counter + 2;
                triangles[counter + 2] = counter + 1;

                vertices[counter + 0] = new Vector3(Convert.ToSingle(vectors[0].x), Convert.ToSingle(vectors[0].y), 0);
                vertices[counter + 1] = new Vector3(Convert.ToSingle(vectors[1].x), Convert.ToSingle(vectors[1].y), 0);
                vertices[counter + 2] = new Vector3(Convert.ToSingle(vectors[2].x), Convert.ToSingle(vectors[2].y), 0);

                if (GradientAxis == 'x')
                {
                    colors[counter + 0] = GetGradient((vectors[0].x - min) / range);
                    colors[counter + 1] = GetGradient((vectors[1].x - min) / range);
                    colors[counter + 2] = GetGradient((vectors[2].x - min) / range);
                }
                else
                {
                    colors[counter + 0] = GetGradient((vectors[0].y - min) / range);
                    colors[counter + 1] = GetGradient((vectors[1].y - min) / range);
                    colors[counter + 2] = GetGradient((vectors[2].y - min) / range);
                }

                counter += 3;
            }

            var mesh = new Mesh();
            _filter.mesh = mesh;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors32 = colors;
            mesh.RecalculateBounds();
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
    }
}
