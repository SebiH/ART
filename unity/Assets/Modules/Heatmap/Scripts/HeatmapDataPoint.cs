using Assets.Modules.Graph;
using UnityEngine;

namespace Assets.Modules.Heatmap
{
    public class HeatmapDataPoint : DataPoint
    {
        private Mesh _mesh;

        void OnEnable()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
            _mesh.vertices = new[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            _mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        }

        void OnDisable()
        {

        }

        public void SetData(float topLeft, float bottomLeft, float bottomRight, float topRight)
        {
            /*
             *  0 - 2
             *  | / |
             *  1 - 3
             */
            _mesh.vertices = new[] {
                new Vector3(-0.5f, topLeft, 0.5f),
                new Vector3(-0.5f, bottomLeft, -0.5f),
                new Vector3(0.5f, topRight, 0.5f),
                new Vector3(0.5f, bottomRight, -0.5f)
            };
            _mesh.RecalculateNormals();
        }
    }
}
