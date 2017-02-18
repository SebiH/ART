using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(MeshFilter))]
    public class GraphicsLineRenderer : MonoBehaviour
    {
        private Mesh _lineMesh;
        private bool _isBusy = false;

        private Queue<LineSegment> _lineCreationQueue = new Queue<LineSegment>();
        private Queue<LineSegment> _lineUpdateQueue = new Queue<LineSegment>();

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
            if (_lineCreationQueue.Count > 0 && !_isBusy)
            {
                StartCoroutine(AddLineAsync());
            }

            if (_lineUpdateQueue.Count > 0 && !_isBusy)
            {
                StartCoroutine(UpdateLineAsync());
            }
        }

        public void AddLine(LineSegment line)
        {
            // adding lines over multiple update()s to avoid extreme lag
            _lineCreationQueue.Enqueue(line);
        }

        public void UpdateLine(LineSegment line)
        {
            _lineUpdateQueue.Enqueue(line);
        }

        private IEnumerator AddLineAsync()
        {
            using (var wd = new WorkDistributor())
            {
                _isBusy = true;
                yield return new WaitForEndOfFrame();

                while (_lineCreationQueue.Count > 0)
                {
                    wd.TriggerUpdate();

                    if (wd.AvailableCycles > 200)
                    {
                        var totalLineNum = _lineCreationQueue.Count;
                        var batchAmount = Mathf.Min(_lineCreationQueue.Count, wd.DepleteAll());

                        var lineBatch = new LineSegment[batchAmount];
                        for (int i = 0; i < batchAmount; i++)
                        {
                            lineBatch[i] = _lineCreationQueue.Dequeue();
                        }

                        CreateLines(lineBatch, totalLineNum);
                    }

                    yield return new WaitForEndOfFrame();
                }

                _isBusy = false;
            }
        }

        private IEnumerator UpdateLineAsync()
        {
            using (var wd = new WorkDistributor())
            {
                _isBusy = true;
                yield return new WaitForEndOfFrame();

                while (_lineUpdateQueue.Count > 0)
                {
                    wd.TriggerUpdate();

                    if (wd.AvailableCycles > 200)
                    {
                        var batchAmount = Mathf.Min(_lineUpdateQueue.Count, wd.DepleteAll());

                        var lineBatch = new LineSegment[batchAmount];
                        for (int i = 0; i < batchAmount; i++)
                        {
                            lineBatch[i] = _lineUpdateQueue.Dequeue();
                        }

                        SetLineAttributes(lineBatch);
                    }

                    yield return new WaitForEndOfFrame();
                }

                _isBusy = false;
            }
        }

        private int vertexCounter = 0;
        private int triangleCounter = 0;

        private void CreateLines(IEnumerable<LineSegment> lines, int expectedLineCount)
        {
            var expectedVerticesNum = vertexCounter + expectedLineCount * 4;
            var currentVerticesNum = _lineMesh.vertices.Length;

            var expectedTriangleNum = triangleCounter + expectedLineCount * 12;
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
            Vector3 widthDirection = Vector3.up;
            Vector3 normal;

            foreach (var line in lines)
            {
                line.MeshIndex = vertexCounter / 4;
                // TODO: could be cached on demand in line
                normal = Vector3.Cross(line.Start, line.End);

                quad[0] = line.Start + widthDirection * line.Width;
                quad[1] = line.Start + widthDirection * -line.Width;
                quad[2] = line.End + widthDirection * line.Width;
                quad[3] = line.End + widthDirection * -line.Width;

                vertices[vertexCounter] = quad[0];
                vertices[vertexCounter + 1] = quad[1];
                vertices[vertexCounter + 2] = quad[2];
                vertices[vertexCounter + 3] = quad[3];

                colors[vertexCounter] = line.Color;
                colors[vertexCounter + 1] = line.Color;
                colors[vertexCounter + 2] = line.Color;
                colors[vertexCounter + 3] = line.Color;


                // front
                triangles[triangleCounter] = vertexCounter;
                triangles[triangleCounter + 1] = vertexCounter + 1;
                triangles[triangleCounter + 2] = vertexCounter + 2;
                triangles[triangleCounter + 3] = vertexCounter + 1;
                triangles[triangleCounter + 4] = vertexCounter + 3;
                triangles[triangleCounter + 5] = vertexCounter + 2;


                // back
                triangles[triangleCounter + 6] = vertexCounter;
                triangles[triangleCounter + 7] = vertexCounter + 2;
                triangles[triangleCounter + 8] = vertexCounter + 1;
                triangles[triangleCounter + 9] = vertexCounter + 1;
                triangles[triangleCounter + 10] = vertexCounter + 2;
                triangles[triangleCounter + 11] = vertexCounter + 3;

                triangleCounter += 12;
                vertexCounter += 4;
            }


            _lineMesh.vertices = vertices;
            _lineMesh.triangles = triangles;
            _lineMesh.colors32 = colors;
            _lineMesh.RecalculateBounds();
        }


        private void SetLineAttributes(IEnumerable<LineSegment> lines)
        {
            var colors = _lineMesh.colors32;
            var vertices = _lineMesh.vertices;

            var quad = new Vector3[4];
            Vector3 widthDirection = Vector3.up;
            Vector3 normal;

            foreach (var line in lines)
            {
                var vertex = line.MeshIndex * 4;

                // TODO: could be cached on demand in line
                normal = Vector3.Cross(line.Start, line.End);

                quad[0] = line.Start + widthDirection * line.Width;
                quad[1] = line.Start + widthDirection * -line.Width;
                quad[2] = line.End + widthDirection * line.Width;
                quad[3] = line.End + widthDirection * -line.Width;

                vertices[vertex] = quad[0];
                vertices[vertex + 1] = quad[1];
                vertices[vertex + 2] = quad[2];
                vertices[vertex + 3] = quad[3];

                colors[vertex] = line.Color;
                colors[vertex + 1] = line.Color;
                colors[vertex + 2] = line.Color;
                colors[vertex + 3] = line.Color;
            }


            _lineMesh.vertices = vertices;
            _lineMesh.colors32 = colors;
            _lineMesh.RecalculateBounds();
        }


        public void ClearLines()
        {
            _lineMesh.Clear();
        }
    }
}
