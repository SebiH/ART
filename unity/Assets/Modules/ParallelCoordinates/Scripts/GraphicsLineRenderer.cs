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
        public Mesh LineMesh { get; set; }
        
        private Queue<LineSegment> _lineCreationQueue = new Queue<LineSegment>();
        private int _creationRoutineId = -1;

        private Queue<LineSegment> _lineVertexUpdateQueue = new Queue<LineSegment>();
        private int _vertexRoutineId = -1;

        private Queue<LineSegment> _lineColorUpdateQueue = new Queue<LineSegment>();
        private int _colorRoutineId = -1;

        private const float DEFAULT_WIDTH = 0.001f;
        private const float FILTERED_WIDTH = 0.0002f;

        private const int MAX_WORK_PER_UPDATE = 200;

        private void OnEnable()
        {
            if (LineMesh == null)
            {
                LineMesh = new Mesh();
                LineMesh.MarkDynamic();
                GetComponent<MeshFilter>().mesh = LineMesh;
            }
        }

        private void LateUpdate()
        {
            if (_lineCreationQueue.Count > 0 && _vertexRoutineId < 0 && _colorRoutineId < 0)
            {
                _creationRoutineId = GameLoop.Instance.StartRoutine(AddLineAsync(), OperationType.Batched);
            }

            if (_lineVertexUpdateQueue.Count > 0 && _creationRoutineId < 0 && _vertexRoutineId < 0)
            {
                _vertexRoutineId = GameLoop.Instance.StartRoutine(UpdateLineVerticesAsync(), OperationType.Batched);
            }

            if (_lineColorUpdateQueue.Count > 0 && _creationRoutineId < 0 && _colorRoutineId < 0)
            {
                _colorRoutineId = GameLoop.Instance.StartRoutine(UpdateLineColorAsync(), OperationType.Batched);
            }
        }

        public void AddLine(LineSegment line)
        {
            // adding lines over multiple update()s to avoid extreme lag
            _lineCreationQueue.Enqueue(line);
        }

        public void UpdateLineVertices(LineSegment line)
        {
            line.WaitingForVertex = true;
            _lineVertexUpdateQueue.Enqueue(line);
        }

        public void UpdateLineColor(LineSegment line)
        {
            line.WaitingForColor = true;
            _lineColorUpdateQueue.Enqueue(line);
        }

        private IEnumerator AddLineAsync()
        {
            while (_lineCreationQueue.Count > 0)
            {
                var batchAmount = Mathf.Min(MAX_WORK_PER_UPDATE, _lineCreationQueue.Count);
                yield return new WaitForAvailableTicks(batchAmount);

                CreateLines(_lineCreationQueue, batchAmount);
            }

            _creationRoutineId = -1;
        }

        private IEnumerator UpdateLineVerticesAsync()
        {
            while (_lineVertexUpdateQueue.Count > 0)
            {
                var lineC = _lineVertexUpdateQueue.Count;
                var batchAmount = Mathf.Min(MAX_WORK_PER_UPDATE, _lineVertexUpdateQueue.Count);
                yield return new WaitForAvailableTicks(batchAmount);
                UpdateLineVertices(_lineVertexUpdateQueue, batchAmount);
            }

            _vertexRoutineId = -1;
        }

        private IEnumerator UpdateLineColorAsync()
        {
            while (_lineColorUpdateQueue.Count > 0)
            {
                var lineC = _lineColorUpdateQueue.Count;
                var batchAmount = Mathf.Min(MAX_WORK_PER_UPDATE, _lineColorUpdateQueue.Count);
                yield return new WaitForAvailableTicks(batchAmount);
                UpdateLineColor(_lineColorUpdateQueue, batchAmount);
            }

            _colorRoutineId = -1;
        }

        private int _vertexCounter = 0;
        private int _triangleCounter = 0;

        private void CreateLines(Queue<LineSegment> lines, int workAmount)
        {
            var expectedVerticesNum = _vertexCounter + lines.Count * 4;
            var currentVerticesNum = LineMesh.vertices.Length;

            var expectedTriangleNum = _triangleCounter + lines.Count * 6;
            var currentTriangleNum = LineMesh.triangles.Length;

            Color32[] colors;
            Vector3[] vertices;
            int[] triangles;

            if (expectedVerticesNum > currentVerticesNum)
            {
                colors = new Color32[expectedVerticesNum];
                Array.Copy(LineMesh.colors32, colors, currentVerticesNum);

                vertices = new Vector3[expectedVerticesNum];
                Array.Copy(LineMesh.vertices, vertices, currentVerticesNum);

                triangles = new int[expectedTriangleNum];
                Array.Copy(LineMesh.triangles, triangles, currentTriangleNum);
            }
            else
            {
                colors = LineMesh.colors32;
                vertices = LineMesh.vertices;
                triangles = LineMesh.triangles;
            }

            Vector3 widthDirection = transform.up;

            for (int i = 0; i < workAmount; i++)
            {
                var line = lines.Dequeue();
                line.WaitingForColor = false;
                line.WaitingForVertex = false;
                line.MeshIndex = _vertexCounter / 4;

                //var width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH;
                var width = DEFAULT_WIDTH;

                vertices[_vertexCounter] = line.Start + widthDirection * width;
                vertices[_vertexCounter + 1] = line.Start + widthDirection * -width;
                vertices[_vertexCounter + 2] = line.End + widthDirection * width;
                vertices[_vertexCounter + 3] = line.End + widthDirection * -width;

                colors[_vertexCounter] = line.Color;
                colors[_vertexCounter + 1] = line.Color;
                colors[_vertexCounter + 2] = line.Color;
                colors[_vertexCounter + 3] = line.Color;

                triangles[_triangleCounter] = _vertexCounter;
                triangles[_triangleCounter + 1] = _vertexCounter + 1;
                triangles[_triangleCounter + 2] = _vertexCounter + 2;
                triangles[_triangleCounter + 3] = _vertexCounter + 1;
                triangles[_triangleCounter + 4] = _vertexCounter + 3;
                triangles[_triangleCounter + 5] = _vertexCounter + 2;

                _triangleCounter += 6;
                _vertexCounter += 4;
            }


            LineMesh.vertices = vertices;
            LineMesh.triangles = triangles;
            LineMesh.colors32 = colors;
            LineMesh.RecalculateBounds();
        }


        private void UpdateLineVertices(Queue<LineSegment> lines, int workAmount)
        {
            var vertices = LineMesh.vertices;

            Vector3 widthDirection = transform.up;

            // micro optimisation: ensure that memory for these variables won't get reinitialised after each loop
            for (int i = 0; i < workAmount; i++)
            {
                var line = lines.Dequeue();
                line.WaitingForVertex = false;
                var vertexIndex = line.MeshIndex * 4;

                //var width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH;
                var width = DEFAULT_WIDTH;

                vertices[vertexIndex] = line.Start + widthDirection * width;
                vertices[vertexIndex + 1] = line.Start + widthDirection * -width;
                vertices[vertexIndex + 2] = line.End + widthDirection * width;
                vertices[vertexIndex + 3] = line.End + widthDirection * -width;
            }

            LineMesh.vertices = vertices;
            LineMesh.RecalculateBounds();
        }


        private void UpdateLineColor(Queue<LineSegment> lines, int workAmount)
        {
            var colors = LineMesh.colors32;

            for (int i = 0; i < workAmount; i++)
            {
                var line = lines.Dequeue();
                line.WaitingForColor = false;
                var vertex = line.MeshIndex * 4;


                colors[vertex] = line.Color;
                colors[vertex + 1] = line.Color;
                colors[vertex + 2] = line.Color;
                colors[vertex + 3] = line.Color;
            }

            LineMesh.colors32 = colors;
        }

        public void SetHidden(bool hidden)
        {
            GetComponent<MeshRenderer>().enabled = !hidden;
        }

        public bool IsHidden()
        {
            return !(GetComponent<MeshRenderer>().enabled);
        }

        public void ClearLines()
        {
            LineMesh.Clear();
            _vertexCounter = 0;
            _triangleCounter = 0;

            if (_creationRoutineId >= 0) { GameLoop.Instance.StopRoutine(_creationRoutineId); _creationRoutineId = -1; }
            _lineCreationQueue.Clear();

            if (_vertexRoutineId >= 0) { GameLoop.Instance.StopRoutine(_vertexRoutineId); _vertexRoutineId = -1; }
            _lineVertexUpdateQueue.Clear();

            if (_colorRoutineId >= 0) { GameLoop.Instance.StopRoutine(_colorRoutineId); _colorRoutineId = -1; }
            _lineColorUpdateQueue.Clear();
        }
    }
}
