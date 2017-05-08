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

        private static float[] ColorOffsets = null;

        // Array position == data index
        public LineProperty[] Lines = new LineProperty[0];

        private void OnEnable()
        {
            _renderer = GetComponent<SkinnedMeshRenderer>();
            _renderer.enabled = false;
        }

        public void Resize(int length)
        {
            if (ColorOffsets == null || ColorOffsets.Length < length)
            {
                ColorOffsets = new float[length];
                for (var i = 0; i < ColorOffsets.Length; i++)
                {
                    ColorOffsets[i] = (i - length / 2f) / (2f * length);
                }
            }

            Debug.Assert(Lines.Length != length, "Performing unnecessary resize()!");
            Lines = new LineProperty[length];
            _mesh = new Mesh();

            // initialize unchanging properties of mesh
            var triangles = new int[Lines.Length * 3];
            var bones = new Transform[Lines.Length * 2];
            var bindPoses = new Matrix4x4[Lines.Length * 2];
            var boneWeights = new BoneWeight[Lines.Length * 2];
            var colors = new Color32[Lines.Length * 2];
            var uv2 = new Vector2[Lines.Length * 2];

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

                triangles[i * 3 + 0] = i * 2 + 0;
                triangles[i * 3 + 1] = i * 2 + 1;
                triangles[i * 3 + 2] = (Lines.Length - 1) * 2;

                bindPoses[i * 2 + 0] = Matrix4x4.identity;
                bindPoses[i * 2 + 1] = Matrix4x4.identity;

                bones[i * 2 + 0] = StartAnchor.transform;
                bones[i * 2 + 1] = EndAnchor.transform;

                boneWeights[i * 2 + 0].weight0 = 1.0f;
                boneWeights[i * 2 + 0].boneIndex0 = i * 2 + 0;
                boneWeights[i * 2 + 1].weight0 = 1.0f;
                boneWeights[i * 2 + 1].boneIndex0 = i * 2 + 1;

                colors[i * 2 + 0] = new Color32(255, 255, 255, 255);
                colors[i * 2 + 1] = new Color32(255, 255, 255, 255);

                uv2[i * 2 + 0].x = ColorOffsets[i];
                uv2[i * 2 + 1].x = ColorOffsets[i];
            }

            triangles[triangles.Length - 1] = 0;

            _mesh.vertices = new Vector3[Lines.Length * 2];
            _mesh.uv2 = uv2;
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
            if (!_mesh)
            {
                return;
            }

            var vertices = _mesh.vertices;
            var colors = _mesh.colors32;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];

                vertices[i * 2 + 0] = new Vector3(line.Start.x, line.Start.y, 0);
                vertices[i * 2 + 1] = new Vector3(line.End.x, line.End.y, 0);

                colors[i * 2 + 0] = line.Color;
                colors[i * 2 + 1] = line.Color;
            }

            _mesh.vertices = vertices;
            _mesh.colors32 = colors;
            _mesh.RecalculateBounds();
        }

        public void UpdatePositions()
        {
            if (!_mesh)
            {
                return;
            }
            var vertices = _mesh.vertices;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                vertices[i * 2 + 0] = new Vector3(line.Start.x, line.Start.y, 0);
                vertices[i * 2 + 1] = new Vector3(line.End.x, line.End.y, 0);
            }

            _mesh.vertices = vertices;
            _mesh.RecalculateBounds();
        }

        public void UpdateColors()
        {
            if (!_mesh)
            {
                return;
            }

            var colors = _mesh.colors32;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                colors[i * 2 + 0] = line.Color;
                colors[i * 2 + 1] = line.Color;
            }

            _mesh.colors32 = colors;
        }

        public void SetHidden(bool isHidden)
        {
            _renderer.enabled = !isHidden;
        }
    }
}
