using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphicsLineRenderer : MonoBehaviour
    {
        public Material LineMaterial;
        private Mesh _lineMesh;
        private const float LINE_WIDTH = 0.005f;

        private struct Line { public Vector3 start; public Vector3 end; }
        private List<Line> _lines = new List<Line>();

        private void OnEnable()
        {
            if (_lineMesh == null)
            {
                _lineMesh = new Mesh();
                GetComponent<MeshFilter>().mesh = _lineMesh;
            }
        }

        private void Update()
        {
            if (_lines.Count > 0)
            {
                var batchAmount = Mathf.Min(_lines.Count, 100);
                var lineBatch = new Line[batchAmount];
                for (int i = 0; i < batchAmount; i++)
                {
                    lineBatch[i] = _lines[0];
                    _lines.RemoveAt(0);
                }
                AddLines(lineBatch);
            }

            // TODO: replaced by MeshFilter, probably needs performance testing?
            //Graphics.DrawMesh(_lineMesh, transform.localToWorldMatrix, LineMaterial, 0);
        }

        public void AddLine(Vector3 start, Vector3 end)
        {
            // adding lines over multiple update()s to avoid extreme lag
            _lines.Add(new Line { start = start, end = end });
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

            // TODO: mesh should never exceed 65536 vertices
            if (currentVerticesCount + quadIndices < 65536)
            {
                var vertices = new Vector3[currentVerticesCount + quadIndices];
                Array.Copy(_lineMesh.vertices, vertices, currentVerticesCount);

                for (int i = 0; i < quadIndices; i++)
                {
                    vertices[currentVerticesCount + i] = quads[i];
                }


                var ts = new int[currentTriangleCount + lines.Length * 6];
                Array.Copy(_lineMesh.triangles, ts, currentTriangleCount);

                for (int i = 0; i < lines.Length; i++)
                {
                    ts[currentTriangleCount + 6 * i] = currentVerticesCount + i;
                    ts[currentTriangleCount + 6 * i + 1] = currentVerticesCount + i + 1;
                    ts[currentTriangleCount + 6 * i + 2] = currentVerticesCount + i + 2;
                    ts[currentTriangleCount + 6 * i + 3] = currentVerticesCount + i + 1;
                    ts[currentTriangleCount + 6 * i + 4] = currentVerticesCount + i + 3;
                    ts[currentTriangleCount + 6 * i + 5] = currentVerticesCount + i + 2;
                }

                _lineMesh.vertices = vertices;
                _lineMesh.triangles = ts;
                _lineMesh.RecalculateBounds();
            }
            else
            {
                Debug.LogWarning("Tried to add too many quads to mesh");
            }

        }

    }
}
