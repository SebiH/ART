using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class SkinnedMeshLineRenderer : MonoBehaviour
    {
        private Mesh _mesh;
        private SkinnedMeshRenderer _renderer;

        public Transform StartAnchor;
        public Transform EndAnchor;

        public struct LineProperty
        {
            public Vector2 Start;
            public Vector2 End;
            public Color32 Color;
        }

        // Array position == data index
        public LineProperty[] Lines = new LineProperty[Globals.DataPointsCount];

        private void OnEnable()
        {
            _mesh = new Mesh();

            _renderer = GetComponent<SkinnedMeshRenderer>();
            _renderer.enabled = false;

            // initialize unchanging properties of mesh
            var triangles = new int[Lines.Length * 3];
            var bones = new Transform[Lines.Length * 3];
            var bindPoses = new Matrix4x4[Lines.Length * 3];
            var boneWeights = new BoneWeight[Lines.Length * 3];
            var colors = new Color32[Lines.Length * 3];

            for (var i = 0; i < Lines.Length; i++)
            {
                /*
                 *  Line composition:
                 *  Width is generated through geometry shader
                 *
                 * 0                    1
                 * o ------------------ o
                 *
                 * |                    |
                 * Bone:               Bone:
                 * Origin              Destination
                 */

                triangles[i * 3 + 0] = i * 3 + 0;
                triangles[i * 3 + 1] = i * 3 + 1;
                triangles[i * 3 + 2] = i * 3 + 2;

                bindPoses[i * 3 + 0] = Matrix4x4.identity;
                bindPoses[i * 3 + 1] = Matrix4x4.identity;
                bindPoses[i * 3 + 2] = Matrix4x4.identity;

                bones[i * 3 + 0] = StartAnchor.transform;
                bones[i * 3 + 1] = EndAnchor.transform;
                bones[i * 3 + 2] = StartAnchor.transform;

                boneWeights[i * 3 + 0].weight0 = 1.0f;
                boneWeights[i * 3 + 0].boneIndex0 = i * 3 + 0;
                boneWeights[i * 3 + 1].weight0 = 1.0f;
                boneWeights[i * 3 + 1].boneIndex0 = i * 3 + 1;
                boneWeights[i * 3 + 2].weight0 = 1.0f;
                boneWeights[i * 3 + 2].boneIndex0 = i * 3 + 0;

                colors[i * 3 + 0] = Lines[i].Color;
                colors[i * 3 + 1] = Lines[i].Color;
                colors[i * 3 + 2] = Lines[i].Color;
            }

            _mesh.vertices = new Vector3[Lines.Length * 3];
            _mesh.colors32 = colors;
            _mesh.triangles = triangles;
            _mesh.boneWeights = boneWeights;
            _mesh.bindposes = bindPoses;
            _mesh.MarkDynamic();

            _renderer.sharedMesh = _mesh;
            _renderer.bones = bones;
        }

        public void GenerateMesh()
        {
            var vertices = _mesh.vertices;
            var colors = _mesh.colors32;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];

                vertices[i * 3 + 0] = new Vector3(line.Start.x, line.Start.y, 0);
                vertices[i * 3 + 1] = new Vector3(line.End.x, line.End.y, 0);
                vertices[i * 3 + 2] = new Vector3(line.Start.x, line.Start.y, 0);

                colors[i * 3 + 0] = line.Color;
                colors[i * 3 + 1] = line.Color;
                colors[i * 3 + 2] = line.Color;
            }

            _mesh.vertices = vertices;
            _mesh.colors32 = colors;
            _mesh.RecalculateBounds();
        }

        public void UpdatePositions()
        {
            var vertices = _mesh.vertices;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                vertices[i * 3 + 0] = new Vector3(line.Start.x, line.Start.y, 0);
                vertices[i * 3 + 1] = new Vector3(line.End.x, line.End.y, 0);
                vertices[i * 3 + 2] = new Vector3(line.Start.x, line.Start.y, 0);
            }

            _mesh.vertices = vertices;
            _mesh.RecalculateBounds();
        }

        public void UpdateColors()
        {
            var colors = _mesh.colors32;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                colors[i * 3 + 0] = line.Color;
                colors[i * 3 + 1] = line.Color;
                colors[i * 3 + 2] = line.Color;
            }

            _mesh.colors32 = colors;
        }

        public void SetHidden(bool isHidden)
        {
            _renderer.enabled = !isHidden;
        }
    }
}
