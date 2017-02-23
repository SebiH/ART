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
        private Mesh _mesh;
        
        private Queue<LineSegment> _lineCreationQueue = new Queue<LineSegment>();
        private int _creationRoutineId = -1;

        private Queue<LineSegment> _lineVertexUpdateQueue = new Queue<LineSegment>();
        private int _vertexRoutineId = -1;

        private Queue<LineSegment> _lineColorUpdateQueue = new Queue<LineSegment>();
        private int _colorRoutineId = -1;

        private const float DEFAULT_WIDTH = 0.001f;
        private const float FILTERED_WIDTH = 0.0002f;

        private const int MAX_WORK_PER_CYCLE = 200;

        private void OnEnable()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.MarkDynamic();
                GetComponent<MeshFilter>().mesh = _mesh;
            }
        }

        private void Update()
        {
            if (_lineCreationQueue.Count > 0 && _vertexRoutineId < 0 && _colorRoutineId < 0)
            {
                _colorRoutineId = GameLoop.Instance.StartRoutine(AddLineAsync(), OperationType.Batched);
            }

            if (_lineVertexUpdateQueue.Count > 0 && _creationRoutineId < 0)
            {
                _vertexRoutineId = GameLoop.Instance.StartRoutine(UpdateLineVerticesAsync(), OperationType.Batched);
            }

            if (_lineColorUpdateQueue.Count > 0 && _creationRoutineId < 0)
            {
                _colorRoutineId = GameLoop.Instance.StartRoutine(UpdateLineVerticesAsync(), OperationType.Batched);
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
                var batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineCreationQueue.Count);
                yield return new WaitForAvailableCycles(batchAmount);

                var totalLineNum = _lineCreationQueue.Count;
                CreateLines(_lineCreationQueue, batchAmount);
            }

            _creationRoutineId = -1;
        }

        private IEnumerator UpdateLineVerticesAsync()
        {
            while (_lineVertexUpdateQueue.Count > 0)
            {
                var lineC = _lineVertexUpdateQueue.Count;
                var batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineVertexUpdateQueue.Count);
                yield return new WaitForAvailableCycles(batchAmount);

                // TODO: setting twice due to strange bug?
                batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineVertexUpdateQueue.Count);
                UpdateLineVertices(_lineVertexUpdateQueue, batchAmount);
            }

            _vertexRoutineId = -1;
        }

        private IEnumerator UpdateLineColorAsync()
        {
            while (_lineColorUpdateQueue.Count > 0)
            {
                var lineC = _lineColorUpdateQueue.Count;
                var batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineColorUpdateQueue.Count);
                yield return new WaitForAvailableCycles(batchAmount);

                // TODO: setting twice due to strange bug?
                batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineColorUpdateQueue.Count);
                UpdateLineColor(_lineColorUpdateQueue, batchAmount);
            }

            _colorRoutineId = -1;
        }

        private int _vertexCounter = 0;
        private int _triangleCounter = 0;

        private void CreateLines(Queue<LineSegment> lines, int workAmount)
        {
            var expectedVerticesNum = _vertexCounter + lines.Count * 4;
            var currentVerticesNum = _mesh.vertices.Length;

            var expectedTriangleNum = _triangleCounter + lines.Count * 6;
            var currentTriangleNum = _mesh.triangles.Length;

            Color32[] colors;
            Vector3[] vertices;
            int[] triangles;

            if (expectedVerticesNum > currentVerticesNum)
            {
                colors = new Color32[expectedVerticesNum];
                Array.Copy(_mesh.colors32, colors, currentVerticesNum);

                vertices = new Vector3[expectedVerticesNum];
                Array.Copy(_mesh.vertices, vertices, currentVerticesNum);

                triangles = new int[expectedTriangleNum];
                Array.Copy(_mesh.triangles, triangles, currentTriangleNum);
            }
            else
            {
                colors = _mesh.colors32;
                vertices = _mesh.vertices;
                triangles = _mesh.triangles;
            }

            Vector3 widthDirection = Vector3.up;

            // micro optimisation: ensure that memory for these variables won't get reinitialised after each loop
            var quad = new Vector3[4];
            Vector3 start, end;
            float width = 0; ;

            for (int i = 0; i < workAmount; i++)
            {
                var line = lines.Dequeue();
                line.WaitingForColor = false;
                line.WaitingForVertex = false;
                line.MeshIndex = _vertexCounter / 4;

                start = new Vector3(-line.Start.x, line.Start.y, line.Start.z);
                end = new Vector3(-line.End.x, line.End.y, line.End.z);
                width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH;

                vertices[_vertexCounter] = start + widthDirection * width;
                vertices[_vertexCounter + 1] = start + widthDirection * -width;
                vertices[_vertexCounter + 2] = end + widthDirection * width;
                vertices[_vertexCounter + 3] = end + widthDirection * -width;

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


            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.colors32 = colors;
            _mesh.RecalculateBounds();
        }


        private void UpdateLineVertices(Queue<LineSegment> lines, int workAmount)
        {
            var vertices = _mesh.vertices;

            Vector3 widthDirection = Vector3.up;

            // micro optimisation: ensure that memory for these variables won't get reinitialised after each loop
            Vector3 start, end;
            float width = 0; ;

            for (int i = 0; i < workAmount; i++)
            {
                var line = lines.Dequeue();
                line.WaitingForVertex = false;
                var vertexIndex = line.MeshIndex * 4;

                start = new Vector3(-line.Start.x, line.Start.y, line.Start.z);
                end = new Vector3(-line.End.x, line.End.y, line.End.z);
                width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH;

                vertices[vertexIndex] = start + widthDirection * width;
                vertices[vertexIndex + 1] = start + widthDirection * -width;
                vertices[vertexIndex + 2] = end + widthDirection * width;
                vertices[vertexIndex + 3] = end + widthDirection * -width;
            }

            _mesh.vertices = vertices;
            _mesh.RecalculateBounds();
        }


        private void UpdateLineColor(Queue<LineSegment> lines, int workAmount)
        {
            var colors = _mesh.colors32;

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

            _mesh.colors32 = colors;
        }


        public void ClearLines()
        {
            _mesh.Clear();
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
