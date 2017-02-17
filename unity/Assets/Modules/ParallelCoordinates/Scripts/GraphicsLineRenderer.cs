using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphicsLineRenderer : MonoBehaviour
    {
        public MeshFilter HighlightedRenderer;
        public MeshFilter FilteredRenderer;

        private Mesh _lineMesh;
        private bool _isGenerating = false;
        private int _lineIndex = 0;
        private const float LINE_WIDTH = 0.005f;

        private List<LineSegment> _lineCreationQueue = new List<LineSegment>();

        private void OnEnable()
        {
            if (_lineMesh == null)
            {
                _lineMesh = new Mesh();
                _lineMesh.MarkDynamic();
                HighlightedRenderer.mesh = _lineMesh;
            }
        }

        private void Update()
        {
            if (_lineCreationQueue.Count > 0 && !_isGenerating)
            {
                StartCoroutine(AddLineAsync());
            }
        }

        public void AddLine(LineSegment line)
        {
            // adding lines over multiple update()s to avoid extreme lag
            _lineCreationQueue.Add(line);
        }

        private IEnumerator AddLineAsync()
        {
            using (var wd = new WorkDistributor())
            {
                _isGenerating = true;
                yield return new WaitForEndOfFrame();

                while (_lineCreationQueue.Count > 0)
                {
                    wd.TriggerUpdate();

                    if (wd.AvailableCycles > 400)
                    {
                        var batchAmount = Mathf.Min(_lineCreationQueue.Count, wd.DepleteAll());

                        var lineBatch = new LineSegment[batchAmount];
                        for (int i = 0; i < batchAmount; i++)
                        {
                            lineBatch[i] = _lineCreationQueue[0];
                            _lineCreationQueue.RemoveAt(0);
                        }
                        AddLines(lineBatch);
                    }

                    yield return new WaitForEndOfFrame();
                }

                _isGenerating = false;
            }
        }

        // TODO: add hint for max lines, so we don't need to do array.copy each time
        private void AddLines(LineSegment[] lines)
        {
            var quads = new Vector3[4 * lines.Length];
            var quadIndices = 0;

            foreach (var line in lines)
            {
                var normal = Vector3.Cross(line.Start, line.End);
                var lineVector = Vector3.Cross(normal, line.End - line.Start);
                lineVector.Normalize();

                quads[quadIndices++] = line.Start + lineVector * LINE_WIDTH;
                quads[quadIndices++] = line.Start + lineVector * -LINE_WIDTH;
                quads[quadIndices++] = line.End + lineVector * LINE_WIDTH;
                quads[quadIndices++] = line.End + lineVector * -LINE_WIDTH;
            }

            var currentVerticesCount = _lineMesh.vertices.Length;
            var currentTriangleCount = _lineMesh.triangles.Length;

            // TODO: mesh cannot exceed 65536 vertices -> split into multiple meshes?
            if (currentVerticesCount + quadIndices < 65536)
            {
                var colors = new Color32[currentVerticesCount + quadIndices];
                Array.Copy(_lineMesh.colors32, colors, currentVerticesCount);
                var vertices = new Vector3[currentVerticesCount + quadIndices];
                Array.Copy(_lineMesh.vertices, vertices, currentVerticesCount);

                var col = new Color32(255, 0, 0, 255);
                for (int i = 0; i < quadIndices; i++)
                {
                    vertices[currentVerticesCount + i] = quads[i];

                    if (i % 4 == 0)
                        col = Theme.GetColor32(UnityEngine.Random.Range(0, 100));
                    colors[currentVerticesCount + i] = col;
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
                _lineMesh.colors32 = colors;
                _lineMesh.RecalculateBounds();
            }
            else
            {
                Debug.LogWarning("Tried to add too many quads to mesh");
            }
        }


        public void SetLineColor(int index, Color32 col)
        {
            if (index < 0)
            {
                Debug.LogWarning("Tried to manipulate line before line was added");
                return;
            }

            // for some reason we cannot use _lineMesh.color32[] directly
            var cols = _lineMesh.colors32;
            if (cols.Length > index * 4 + 4) // TODO: ... should not occur??
            {
                cols[index * 4] = col;
                cols[index * 4 + 1] = col;
                cols[index * 4 + 2] = col;
                cols[index * 4 + 3] = col;
                _lineMesh.colors32 = cols;
            }
        }

        public void ClearLines()
        {
            _lineMesh.Clear();
        }
    }
}
