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
                    var totalLineNum = _lineCreationQueue.Count;

                    if (wd.AvailableCycles > 400)
                    {
                        var batchAmount = Mathf.Min(_lineCreationQueue.Count, wd.DepleteAll());

                        var lineBatch = new LineSegment[batchAmount];
                        for (int i = 0; i < batchAmount; i++)
                        {
                            lineBatch[i] = _lineCreationQueue[0];
                            _lineCreationQueue.RemoveAt(0);
                        }
                        AddLines(lineBatch, totalLineNum);
                    }

                    yield return new WaitForEndOfFrame();
                }

                _isGenerating = false;
            }
        }

        private int vertexCounter = 0;
        private int triangleCounter = 0;


        private void AddLines(LineSegment[] lines, int expectedLineCount)
        {
            var expectedVerticesNum = vertexCounter + expectedLineCount * 4;
            var currentVerticesNum = _lineMesh.vertices.Length;

            var expectedTriangleNum = triangleCounter + expectedLineCount * 6;
            var currentTriangleNum = _lineMesh.triangles.Length;

            Color32[] colors;
            Vector3[] vertices;
            int[] triangles;

            if (expectedVerticesNum > currentVerticesNum)
            {
                colors = new Color32[expectedVerticesNum];
                Array.Copy(_lineMesh.colors32, colors, currentVerticesNum);
                vertices = new Vector3[expectedVerticesNum];
                Array.Copy(_lineMesh.vertices, vertices, currentVerticesNum);

                triangles = new int[expectedTriangleNum];
                Array.Copy(_lineMesh.triangles, triangles, currentTriangleNum);
            }
            else
            {
                colors = _lineMesh.colors32;
                vertices = _lineMesh.vertices;
                triangles = _lineMesh.triangles;
            }


            var quad = new Vector3[4];
            Vector3 normal, ortho;
            int indexOffset = currentVerticesNum / 4;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                line.MeshIndex = indexOffset + i;
                normal = Vector3.Cross(line.Start, line.End);
                ortho = Vector3.Cross(normal, line.End - line.Start);
                ortho.Normalize();

                quad[0] = line.Start + ortho * line.Width;
                quad[1] = line.Start + ortho * -line.Width;
                quad[2] = line.End + ortho * line.Width;
                quad[3] = line.End + ortho * -line.Width;

                vertices[vertexCounter] = quad[0];
                vertices[vertexCounter + 1] = quad[1];
                vertices[vertexCounter + 2] = quad[2];
                vertices[vertexCounter + 3] = quad[3];

                colors[vertexCounter] = line.Color;
                colors[vertexCounter + 1] = line.Color;
                colors[vertexCounter + 2] = line.Color;
                colors[vertexCounter + 3] = line.Color;

                triangles[triangleCounter] = vertexCounter;
                triangles[triangleCounter + 1] = vertexCounter + 1;
                triangles[triangleCounter + 2] = vertexCounter + 2;
                triangles[triangleCounter + 3] = vertexCounter + 1;
                triangles[triangleCounter + 4] = vertexCounter + 3;
                triangles[triangleCounter + 5] = vertexCounter + 2;

                vertexCounter += 4;
                triangleCounter += 6;
            }


            _lineMesh.vertices = vertices;
            _lineMesh.triangles = triangles;
            _lineMesh.colors32 = colors;
            _lineMesh.RecalculateBounds();
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
