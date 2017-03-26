using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates_Deprecated
{
    public class SkinnedLineRenderer : MonoBehaviour
    {
        public MeshFilter NormalFilter;
        public MeshFilter TransparentFilter;
        public SkinnedMeshRenderer SkinnedRenderer;

        private Mesh _normalMesh;
        private Mesh _transparentMesh;

        private Queue<SkinnedLineSegment> _lineCreationQueue = new Queue<SkinnedLineSegment>();
        private int _creationRoutineId = -1;

        private Queue<SkinnedLineSegment> _lineColorUpdateQueue = new Queue<SkinnedLineSegment>();
        private int _colorRoutineId = -1;

        private const float DEFAULT_WIDTH = 0.001f;
        private const float FILTERED_WIDTH = 0.0002f;

        private const int MAX_WORK_PER_CYCLE = 200;

        private void OnEnable()
        {
            //if (_transparentMesh == null)
            //{
            //    _transparentMesh = new Mesh();
            //    _transparentMesh.MarkDynamic();
            //    TransparentFilter.mesh = _transparentMesh;
            //}

            //if (_normalMesh == null)
            //{
            //    _normalMesh = new Mesh();
            //    _normalMesh.MarkDynamic();
            //    NormalFilter.mesh = _normalMesh;
            //}
        }

        private void Update()
        {
            //if (_lineCreationQueue.Count > 0 && _vertexRoutineId < 0 && _colorRoutineId < 0)
            //{
            //    _colorRoutineId = GameLoop.Instance.StartRoutine(AddLineAsync(), OperationType.Batched);
            //}

            if (_lineCreationQueue.Count > 0)
            {
                AddLines(_lineCreationQueue);
            }

            //if (_lineVertexUpdateQueue.Count > 0 && _creationRoutineId < 0)
            //{
            //    _vertexRoutineId = GameLoop.Instance.StartRoutine(UpdateLineVerticesAsync(), OperationType.Batched);
            //}

            //if (_lineColorUpdateQueue.Count > 0 && _creationRoutineId < 0)
            //{
            //    _colorRoutineId = GameLoop.Instance.StartRoutine(UpdateLineVerticesAsync(), OperationType.Batched);
            //}
        }

        public void AddLine(SkinnedLineSegment line)
        {
            // adding lines over multiple update()s to avoid extreme lag
            _lineCreationQueue.Enqueue(line);
        }


        public void AddLines(Queue<SkinnedLineSegment> lines)
        {
            var totalVertices = lines.Count * 4;
            var totalTriangles = lines.Count * 6;

            var vertices = new Vector3[totalVertices];
            var colors = new Color32[totalVertices];
            var bindPoses = new Matrix4x4[lines.Count];
            var bones = new Transform[lines.Count];
            var boneWeights = new BoneWeight[totalVertices];
            var triangles = new int[totalTriangles];

            var vertexOffset = 0;
            var triangleOffset = 0;
            var lineOffset = 0;

            Vector3 start, end;
            float width = 0; ;
            var widthDirection = Vector3.up;

            while (lines.Count > 0)
            {
                var line = lines.Dequeue();

                line.WaitingForColor = false;
                line.WaitingForVertex = false;
                line.MeshIndex = _vertexCounter / 4;

                start = new Vector3(-line.Start.x, line.Start.y, line.Start.z);
                end = new Vector3(-line.End.x, line.End.y, line.End.z);
                width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH;

                vertices[vertexOffset] = start + widthDirection * width;
                vertices[vertexOffset + 1] = start + widthDirection * -width;
                vertices[vertexOffset + 2] = end + widthDirection * width;
                vertices[vertexOffset + 3] = end + widthDirection * -width;

                bindPoses[lineOffset] = Matrix4x4.identity;
                bones[lineOffset] = line.go.transform;
                for (int i = 0; i < 4; i++)
                {
                    boneWeights[vertexOffset + i].weight0 = 1.0f;
                    boneWeights[vertexOffset + i].boneIndex0 = lineOffset;
                }

                //quadTransparent[0] = start + widthDirection * width;
                //quadTransparent[1] = start + widthDirection * -width;
                //quadTransparent[2] = (line.IsFiltered ? end : start) + widthDirection * width;
                //quadTransparent[3] = (line.IsFiltered ? end : start) + widthDirection * -width;

                //verticesNormal[_vertexCounter] = quadNormal[0];
                //verticesNormal[_vertexCounter + 1] = quadNormal[1];
                //verticesNormal[_vertexCounter + 2] = quadNormal[2];
                //verticesNormal[_vertexCounter + 3] = quadNormal[3];

                //verticesTransparent[_vertexCounter] = quadTransparent[0];
                //verticesTransparent[_vertexCounter + 1] = quadTransparent[1];
                //verticesTransparent[_vertexCounter + 2] = quadTransparent[2];
                //verticesTransparent[_vertexCounter + 3] = quadTransparent[3];

                colors[vertexOffset] = line.Color;
                colors[vertexOffset + 1] = line.Color;
                colors[vertexOffset + 2] = line.Color;
                colors[vertexOffset + 3] = line.Color;


                // front
                triangles[triangleOffset] = vertexOffset;
                triangles[triangleOffset + 1] = vertexOffset + 1;
                triangles[triangleOffset + 2] = vertexOffset + 2;
                triangles[triangleOffset + 3] = vertexOffset + 1;
                triangles[triangleOffset + 4] = vertexOffset + 3;
                triangles[triangleOffset + 5] = vertexOffset + 2;

                lineOffset++;
                vertexOffset += 4;
                triangleOffset += 6;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.boneWeights = boneWeights;
            mesh.triangles = triangles;
            mesh.colors32 = colors;
            mesh.bindposes = bindPoses;

            SkinnedRenderer.sharedMesh = mesh;
            SkinnedRenderer.bones = bones;
        }










        public void UpdateLineColor(SkinnedLineSegment line)
        {
            line.WaitingForColor = true;
            _lineColorUpdateQueue.Enqueue(line);
        }

        private IEnumerator AddLineAsync()
        {
            while (_lineCreationQueue.Count > 0)
            {
                var batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineCreationQueue.Count);
                yield return new WaitForAvailableTicks(batchAmount);

                var totalLineNum = _lineCreationQueue.Count;
                CreateLines(_lineCreationQueue, batchAmount);
            }

            _creationRoutineId = -1;
        }

        private IEnumerator UpdateLineColorAsync()
        {
            while (_lineColorUpdateQueue.Count > 0)
            {
                var lineC = _lineColorUpdateQueue.Count;
                var batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineColorUpdateQueue.Count);
                yield return new WaitForAvailableTicks(batchAmount);

                // TODO: setting twice due to strange bug?
                batchAmount = Mathf.Min(MAX_WORK_PER_CYCLE, _lineColorUpdateQueue.Count);
                UpdateLineColor(_lineColorUpdateQueue, batchAmount);
            }

            _colorRoutineId = -1;
        }

        private int _vertexCounter = 0;
        private int _triangleCounter = 0;

        private void CreateLines(Queue<SkinnedLineSegment> lines, int workAmount)
        {
            var expectedVerticesNum = _vertexCounter + lines.Count * 4;
            var currentVerticesNum = _normalMesh.vertices.Length;

            var expectedTriangleNum = _triangleCounter + lines.Count * 6;
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

            for (int i = 0; i < workAmount; i++)
            {
                var line = lines.Dequeue();
                line.WaitingForColor = false;
                line.WaitingForVertex = false;
                line.MeshIndex = _vertexCounter / 4;

                start = new Vector3(-line.Start.x, line.Start.y, line.Start.z);
                end = new Vector3(-line.End.x, line.End.y, line.End.z);
                width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH;

                quadNormal[0] = start + widthDirection * width;
                quadNormal[1] = start + widthDirection * -width;
                quadNormal[2] = (line.IsFiltered ? start : end) + widthDirection * width;
                quadNormal[3] = (line.IsFiltered ? start : end) + widthDirection * -width;

                quadTransparent[0] = start + widthDirection * width;
                quadTransparent[1] = start + widthDirection * -width;
                quadTransparent[2] = (line.IsFiltered ? end : start) + widthDirection * width;
                quadTransparent[3] = (line.IsFiltered ? end : start) + widthDirection * -width;

                verticesNormal[_vertexCounter] = quadNormal[0];
                verticesNormal[_vertexCounter + 1] = quadNormal[1];
                verticesNormal[_vertexCounter + 2] = quadNormal[2];
                verticesNormal[_vertexCounter + 3] = quadNormal[3];

                verticesTransparent[_vertexCounter] = quadTransparent[0];
                verticesTransparent[_vertexCounter + 1] = quadTransparent[1];
                verticesTransparent[_vertexCounter + 2] = quadTransparent[2];
                verticesTransparent[_vertexCounter + 3] = quadTransparent[3];

                colors[_vertexCounter] = line.Color;
                colors[_vertexCounter + 1] = line.Color;
                colors[_vertexCounter + 2] = line.Color;
                colors[_vertexCounter + 3] = line.Color;


                // front
                triangles[_triangleCounter] = _vertexCounter;
                triangles[_triangleCounter + 1] = _vertexCounter + 1;
                triangles[_triangleCounter + 2] = _vertexCounter + 2;
                triangles[_triangleCounter + 3] = _vertexCounter + 1;
                triangles[_triangleCounter + 4] = _vertexCounter + 3;
                triangles[_triangleCounter + 5] = _vertexCounter + 2;


                // back
                //triangles[triangleCounter + 6] = vertexCounter;
                //triangles[triangleCounter + 7] = vertexCounter + 2;
                //triangles[triangleCounter + 8] = vertexCounter + 1;
                //triangles[triangleCounter + 9] = vertexCounter + 1;
                //triangles[triangleCounter + 10] = vertexCounter + 2;
                //triangles[triangleCounter + 11] = vertexCounter + 3;

                _triangleCounter += 6;
                _vertexCounter += 4;
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


        private void UpdateLineVertices(Queue<SkinnedLineSegment> lines, int workAmount)
        {
            var verticesNormal = _normalMesh.vertices;
            var verticesTransparent = _transparentMesh.vertices;

            Vector3 widthDirection = Vector3.up;

            // micro optimisation: ensure that memory for these variables won't get reinitialised after each loop
            var quadNormal = new Vector3[4];
            var quadTransparent = new Vector3[4];
            Vector3 start, end;
            float width = 0; ;

            for (int i = 0; i < workAmount; i++)
            {
                var line = lines.Dequeue();
                line.WaitingForVertex = false;
                var vertex = line.MeshIndex * 4;

                start = new Vector3(-line.Start.x, line.Start.y, line.Start.z);
                end = new Vector3(-line.End.x, line.End.y, line.End.z);
                width = line.IsFiltered ? FILTERED_WIDTH : DEFAULT_WIDTH;

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
            }

            _normalMesh.vertices = verticesNormal;
            _normalMesh.RecalculateBounds();

            _transparentMesh.vertices = verticesTransparent;
            _transparentMesh.RecalculateBounds();
        }


        private void UpdateLineColor(Queue<SkinnedLineSegment> lines, int workAmount)
        {
            var colors = _normalMesh.colors32;

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

            _normalMesh.colors32 = colors;
            _transparentMesh.colors32 = colors;
        }


        public void ClearLines()
        {
            _normalMesh.Clear();
            _transparentMesh.Clear();
            _vertexCounter = 0;
            _triangleCounter = 0;

            if (_creationRoutineId >= 0) { GameLoop.Instance.StopRoutine(_creationRoutineId); _creationRoutineId = -1; }
            _lineCreationQueue.Clear();

            //if (_vertexRoutineId >= 0) { GameLoop.Instance.StopRoutine(_vertexRoutineId); _vertexRoutineId = -1; }
            //_lineVertexUpdateQueue.Clear();

            if (_colorRoutineId >= 0) { GameLoop.Instance.StopRoutine(_colorRoutineId); _colorRoutineId = -1; }
            _lineColorUpdateQueue.Clear();
        }

    }
}
