using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphicsLineRenderer : MonoBehaviour
    {
        public MeshFilter NormalFilter;
        public MeshFilter TransparentFilter;

        private Mesh _normalMesh;
        private Mesh _transparentMesh;
        private bool _isBusy = false;

        private Queue<LineSegment> _lineCreationQueue = new Queue<LineSegment>();
        private Queue<LineSegment> _lineUpdateQueue = new Queue<LineSegment>();

        private const float DEFAULT_WIDTH = 0.001f;
        private const float FILTERED_WIDTH = 0.0002f;

        private void OnEnable()
        {
            if (_transparentMesh == null)
            {
                _transparentMesh = new Mesh();
                _transparentMesh.MarkDynamic();
                TransparentFilter.mesh = _transparentMesh;
            }

            if (_normalMesh == null)
            {
                _normalMesh = new Mesh();
                _normalMesh.MarkDynamic();
                NormalFilter.mesh = _normalMesh;
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
            line.WaitingForUpdate = true;
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
                            lineBatch[i].WaitingForUpdate = false;
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
                            lineBatch[i].WaitingForUpdate = false;
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
            var currentVerticesNum = _normalMesh.vertices.Length;

            var expectedTriangleNum = triangleCounter + expectedLineCount * 6;
            var currentTriangleNum = _normalMesh.triangles.Length;

            Color32[] colors;
            Vector3[] verticesNormal, verticesTransparent;
            int[] triangles;

            if (expectedVerticesNum > currentVerticesNum)
            {
                colors = new Color32[expectedVerticesNum];
                Array.Copy(_normalMesh.colors32, colors, currentVerticesNum);

                verticesNormal = new Vector3[expectedVerticesNum];
                Array.Copy(_normalMesh.vertices, verticesNormal, currentVerticesNum);
                verticesTransparent = new Vector3[expectedVerticesNum];
                Array.Copy(_transparentMesh.vertices, verticesTransparent, currentVerticesNum);

                triangles = new int[expectedTriangleNum];
                Array.Copy(_normalMesh.triangles, triangles, currentTriangleNum);
            }
            else
            {
                colors = _normalMesh.colors32;
                verticesNormal = _normalMesh.vertices;
                verticesTransparent = _transparentMesh.vertices;
                triangles = _normalMesh.triangles;
            }


            Vector3 widthDirection = Vector3.up;

            // micro optimisation: ensure that memory for these variables won't get reinitialised after each loop
            var quadNormal = new Vector3[4];
            var quadTransparent = new Vector3[4];
            Vector3 start, end;
            float width = 0; ;

            foreach (var line in lines)
            {
                line.MeshIndex = vertexCounter / 4;

                start = new Vector3(-line.Start.x, line.Start.y, line.Start.z);
                end = new Vector3(-line.End.x, line.End.y, line.End.z);
                width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH ;

                quadNormal[0] = start + widthDirection * width;
                quadNormal[1] = start + widthDirection * -width;
                quadNormal[2] = (line.IsFiltered ? start : end) + widthDirection * width;
                quadNormal[3] = (line.IsFiltered ? start : end) + widthDirection * -width;

                quadTransparent[0] = start + widthDirection * width;
                quadTransparent[1] = start + widthDirection * -width;
                quadTransparent[2] = (line.IsFiltered ? end : start) + widthDirection * width;
                quadTransparent[3] = (line.IsFiltered ? end : start) + widthDirection * -width;

                verticesNormal[vertexCounter] = quadNormal[0];
                verticesNormal[vertexCounter + 1] = quadNormal[1];
                verticesNormal[vertexCounter + 2] = quadNormal[2];
                verticesNormal[vertexCounter + 3] = quadNormal[3];

                verticesTransparent[vertexCounter] = quadTransparent[0];
                verticesTransparent[vertexCounter + 1] = quadTransparent[1];
                verticesTransparent[vertexCounter + 2] = quadTransparent[2];
                verticesTransparent[vertexCounter + 3] = quadTransparent[3];

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
                //triangles[triangleCounter + 6] = vertexCounter;
                //triangles[triangleCounter + 7] = vertexCounter + 2;
                //triangles[triangleCounter + 8] = vertexCounter + 1;
                //triangles[triangleCounter + 9] = vertexCounter + 1;
                //triangles[triangleCounter + 10] = vertexCounter + 2;
                //triangles[triangleCounter + 11] = vertexCounter + 3;

                triangleCounter += 6;
                vertexCounter += 4;
            }


            _normalMesh.vertices = verticesNormal;
            _normalMesh.triangles = triangles;
            _normalMesh.colors32 = colors;
            _normalMesh.RecalculateBounds();

            _transparentMesh.vertices = verticesTransparent;
            _transparentMesh.triangles = triangles;
            _transparentMesh.colors32 = colors;
            _transparentMesh.RecalculateBounds();
        }


        private void SetLineAttributes(IEnumerable<LineSegment> lines)
        {
            var colors = _normalMesh.colors32;
            var verticesNormal = _normalMesh.vertices;
            var verticesTransparent = _transparentMesh.vertices;

            Vector3 widthDirection = Vector3.up;

            // micro optimisation: ensure that memory for these variables won't get reinitialised after each loop
            var quadNormal = new Vector3[4];
            var quadTransparent = new Vector3[4];
            Vector3 start, end;
            float width = 0; ;

            foreach (var line in lines)
            {
                var vertex = line.MeshIndex * 4;

                start = new Vector3(-line.Start.x, line.Start.y, line.Start.z);
                end = new Vector3(-line.End.x, line.End.y, line.End.z);
                width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH ;

                quadNormal[0] = start + widthDirection * width;
                quadNormal[1] = start + widthDirection * -width;
                quadNormal[2] = (line.IsFiltered ? start : end) + widthDirection * width;
                quadNormal[3] = (line.IsFiltered ? start : end) + widthDirection * -width;

                quadTransparent[0] = start + widthDirection * width;
                quadTransparent[1] = start + widthDirection * -width;
                quadTransparent[2] = (line.IsFiltered ? end : start) + widthDirection * width;
                quadTransparent[3] = (line.IsFiltered ? end : start) + widthDirection * -width;

                verticesNormal[vertex] = quadNormal[0];
                verticesNormal[vertex + 1] = quadNormal[1];
                verticesNormal[vertex + 2] = quadNormal[2];
                verticesNormal[vertex + 3] = quadNormal[3];

                verticesTransparent[vertex] = quadTransparent[0];
                verticesTransparent[vertex + 1] = quadTransparent[1];
                verticesTransparent[vertex + 2] = quadTransparent[2];
                verticesTransparent[vertex + 3] = quadTransparent[3];

                colors[vertex] = line.Color;
                colors[vertex + 1] = line.Color;
                colors[vertex + 2] = line.Color;
                colors[vertex + 3] = line.Color;
            }


            _normalMesh.vertices = verticesNormal;
            _normalMesh.colors32 = colors;
            _normalMesh.RecalculateBounds();

            _transparentMesh.vertices = verticesTransparent;
            _transparentMesh.colors32 = colors;
            _transparentMesh.RecalculateBounds();
        }


        public void ClearLines()
        {
            _normalMesh.Clear();
            _transparentMesh.Clear();
        }
    }
}
