using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(MeshFilter), typeof(SkinnedMeshRenderer))]
    public class SkinnedMeshLineRenderer : MonoBehaviour
    {
        private readonly Mesh _mesh = new Mesh();
        private MeshFilter _filter;
        private MeshRenderer _renderer;

        // attached to destination graph, to avoid changing bone transform
        private GraphTracker _destinationAnchor;

        public struct LineProperty
        {
            public Vector2 Start;
            public Vector2 End;
            public Color32 Color;
            public float Size;
        }

        // Array position == data index
        public LineProperty[] Lines = new LineProperty[Globals.DataPointsCount];

        private void OnEnable()
        {
            var x = new LineProperty();
            Debug.LogError(System.Runtime.InteropServices.Marshal.SizeOf(x));

            _filter = GetComponent<MeshFilter>();
            _filter.mesh = _mesh;

            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;

            _destinationAnchor = GetComponentInChildren<GraphTracker>();
            if (!_destinationAnchor) { Debug.LogError("SkinnedMeshLineRenderer needs GraphTracker in as childobject"); }

            // initialize unchanging properties of mesh
            var triangles = new int[Lines.Length * 6];
            var bones = new Transform[Lines.Length * 2];
            var bindPoses = new Matrix4x4[Lines.Length * 2];
            var boneWeights = new BoneWeight[Lines.Length * 4];

            for (var i = 0; i < Lines.Length; i++)
            {
                /*
                 *  Line composition:
                 *
                 *
                 * 0                    1
                 * o ------------------ o
                 * |                  / |
                 * |         /          |
                 * | /                  |
                 * o ------------------ o
                 * 2                    3
                 *
                 * |                    |
                 * Bone:               Bone:
                 * Origin              Destination
                 */

                triangles[i * 6 + 0] = i;
                triangles[i * 6 + 1] = i + 1;
                triangles[i * 6 + 2] = i + 2;
                triangles[i * 6 + 3] = i + 1;
                triangles[i * 6 + 4] = i + 3;
                triangles[i * 6 + 5] = i + 2;

                bindPoses[i * 2 + 0] = Matrix4x4.identity;
                bindPoses[i * 2 + 1] = Matrix4x4.identity;

                bones[i * 2 + 0] = transform;
                bones[i * 2 + 1] = _destinationAnchor.transform;

                boneWeights[i * 4 + 0].weight0 = 1.0f;
                boneWeights[i * 4 + 0].boneIndex0 = i * 2 + 0;
                boneWeights[i * 4 + 1].weight0 = 1.0f;
                boneWeights[i * 4 + 1].boneIndex0 = i * 2 + 1;
                boneWeights[i * 4 + 2].weight0 = 1.0f;
                boneWeights[i * 4 + 2].boneIndex0 = i * 2 + 0;
                boneWeights[i * 4 + 3].weight0 = 1.0f;
                boneWeights[i * 4 + 3].boneIndex0 = i * 2 + 1;

                Lines[i] = new LineProperty
                {
                    Start = new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0),
                    End = new Vector3(Random.value - 0.5f, Random.value - 0.5f, 1),
                    Color = new Color32(255, 255, 255, 255),
                    Size = 0.01f
                };
            }

            _mesh.vertices = new Vector3[Lines.Length * 4];
            _mesh.colors32 = new Color32[Lines.Length * 4];
            _mesh.triangles = triangles;
            _mesh.MarkDynamic();
        }

        private void GenerateMesh()
        {
            _renderer.enabled = true;

            var vertices = _mesh.vertices;
            var colors = _mesh.colors32;

            for (var i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                vertices[i * 4 + 0] = new Vector3(line.Start.x, line.Start.y - line.Size, 0);
                vertices[i * 4 + 1] = new Vector3(line.End.x, line.End.y + line.Size, 1);
                vertices[i * 4 + 2] = new Vector3(line.Start.x, line.Start.y - line.Size, 0);
                vertices[i * 4 + 3] = new Vector3(line.End.x, line.End.y - line.Size, 1);

                colors[i * 4 + 0] = line.Color;
                colors[i * 4 + 1] = line.Color;
                colors[i * 4 + 2] = line.Color;
                colors[i * 4 + 3] = line.Color;
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
                vertices[i * 4 + 0] = new Vector3(line.Start.x, line.Start.y - line.Size, 0);
                vertices[i * 4 + 1] = new Vector3(line.End.x, line.End.y + line.Size, 1);
                vertices[i * 4 + 2] = new Vector3(line.Start.x, line.Start.y - line.Size, 0);
                vertices[i * 4 + 3] = new Vector3(line.End.x, line.End.y - line.Size, 1);
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
                colors[i * 4 + 0] = line.Color;
                colors[i * 4 + 1] = line.Color;
                colors[i * 4 + 2] = line.Color;
                colors[i * 4 + 3] = line.Color;
            }

            _mesh.colors32 = colors;
        }

        public void SetHidden(bool isHidden)
        {
            _renderer.enabled = !isHidden;
        }
    }
}
