using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphicsLineRenderer : MonoBehaviour
    {
        public Material LineMaterial;
        private Mesh _lineMesh;
        private bool _isGenerating = false;
        private const float LINE_WIDTH = 0.005f;

        private struct Line { public Vector3 start; public Vector3 end; }
        private List<Line> _lines = new List<Line>();

        private void OnEnable()
        {
            if (_lineMesh == null)
            {
                _lineMesh = new Mesh();
                _lineMesh.MarkDynamic();
                GetComponent<MeshFilter>().mesh = _lineMesh;
            }
        }

        private void Update()
        {
            if (_lines.Count > 0 && !_isGenerating)
            {
                StartCoroutine(AddLineAsync());
            }

            // TODO: replaced by MeshFilter, probably needs performance testing?
            //Graphics.DrawMesh(_lineMesh, transform.localToWorldMatrix, LineMaterial, 0);
        }

        public void AddLine(Vector3 start, Vector3 end)
        {
            // adding lines over multiple update()s to avoid extreme lag
            _lines.Add(new Line { start = start, end = end });
        }

        private IEnumerator AddLineAsync()
        {
            using (var wd = new WorkDistributor())
            {
                _isGenerating = true;
                yield return new WaitForEndOfFrame();

                while (_lines.Count > 0)
                {
                    wd.TriggerUpdate();

                    if (wd.AvailableCycles > 400)
                    {
                        var batchAmount = Mathf.Min(_lines.Count, wd.DepleteAll());

                        var lineBatch = new Line[batchAmount];
                        for (int i = 0; i < batchAmount; i++)
                        {
                            lineBatch[i] = _lines[0];
                            _lines.RemoveAt(0);
                        }
                        AddLines(lineBatch);
                    }

                    yield return new WaitForEndOfFrame();
                }

                _isGenerating = false;
            }
        }

        private void AddLines(Line[] lines)
        {
            var quads = new Vector3[4 * lines.Length];
            var quadIndices = 0;

            foreach (var line in lines)
            {
                var normal = Vector3.Cross(line.start, line.end);
                var lineVector = Vector3.Cross(normal, line.end - line.start);
                lineVector.Normalize();

                quads[quadIndices++] = line.start + lineVector * LINE_WIDTH;
                quads[quadIndices++] = line.start + lineVector * -LINE_WIDTH;
                quads[quadIndices++] = line.end + lineVector * LINE_WIDTH;
                quads[quadIndices++] = line.end + lineVector * -LINE_WIDTH;
            }

            var currentVerticesCount = _lineMesh.vertices.Length;
            var currentTriangleCount = _lineMesh.triangles.Length;

            // TODO: mesh cannot exceed 65536 vertices -> split into multiple meshes?
            if (currentVerticesCount + quadIndices < 65536)
            {
                var vertices = new Vector3[currentVerticesCount + quadIndices];
                Array.Copy(_lineMesh.vertices, vertices, currentVerticesCount);

                for (int i = 0; i < quadIndices; i++)
                {
                    vertices[currentVerticesCount + i] = quads[i];
                }


                var triangles = new int[currentTriangleCount + lines.Length * 6];
                Array.Copy(_lineMesh.triangles, triangles, currentTriangleCount);

                for (int i = 0; i < lines.Length; i++)
                {
                    triangles[currentTriangleCount + 6 * i] = currentVerticesCount + i *  4;
                    triangles[currentTriangleCount + 6 * i + 1] = currentVerticesCount + i * 4 + 1;
                    triangles[currentTriangleCount + 6 * i + 2] = currentVerticesCount + i * 4 + 2;
                    triangles[currentTriangleCount + 6 * i + 3] = currentVerticesCount + i * 4 + 1;
                    triangles[currentTriangleCount + 6 * i + 4] = currentVerticesCount + i * 4 + 3;
                    triangles[currentTriangleCount + 6 * i + 5] = currentVerticesCount + i * 4 + 2;
                }

                _lineMesh.vertices = vertices;
                _lineMesh.triangles = triangles;
                _lineMesh.RecalculateBounds();
            }
            else
            {
                Debug.LogWarning("Tried to add too many quads to mesh");
            }

        }

    }
}
