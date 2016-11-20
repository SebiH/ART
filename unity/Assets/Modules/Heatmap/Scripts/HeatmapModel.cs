using UnityEngine;

namespace Assets.Modules.Heatmap
{
    public class HeatmapModel : MonoBehaviour
    {
        private Mesh _mesh;
        void OnEnable()
        {
            var filter = GetComponent<MeshFilter>();
            _mesh = new Mesh();
            filter.mesh = _mesh;
        }

        public void SetMesh(Vector3[] vertices, int[] triangles)
        {
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
        }
    }
}
