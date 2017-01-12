using UnityEngine;
using System.Collections.Generic;

namespace Assets.Deprecated.Graph
{
    public class PlaneGraph : MonoBehaviour
    {

        [Range(1, 200)]
        public int width;

        [Range(1, 200)]
        public int height;

        // data that will be visualised
        private float[,] data;

        // generated mesh
        private List<Vector3> vertices = new List<Vector3>();
        // TODO: dynamic?
        private int[,] vertexIndices;
        private List<int> triangles = new List<int>();

        private Mesh mesh;

        // Use this for initialization
        void Start()
        {
        }


        void Update()
        {
            // generate random data on left mouse click
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                data = new float[width, height];

                for (int x = 0; x < data.GetLength(0); x++)
                {
                    for (int y = 0; y < data.GetLength(1); y++)
                    {
                        data[x, y] = Random.Range(0f, 5f);
                    }
                }

                RegenerateMesh();
            }
        }

        void OnDrawGizmos()
        {
            //foreach (var vertex in vertices)
            //{
            //    Gizmos.DrawSphere(vertex, 0.1f);
            //}
        }


        private void GenerateMesh()
        {
            var columns = data.GetLength(0);
            var rows = data.GetLength(1);

            vertices.Clear();
            triangles.Clear();

            vertexIndices = new int[columns, rows];
            var vertexIndicesCounter = 0;

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var position = new Vector3(x, data[x, y], y);
                    vertices.Add(position);

                    vertexIndices[x, y] = vertexIndicesCounter;
                    vertexIndicesCounter++;

                    bool isFirstRow = (y == 0);
                    bool isFirstColumn = (x == 0);

                    if (!isFirstRow && !isFirstColumn)
                    {
                        /*
                         *  Connect neighbouring vertices:
                         *  0 - 2
                         *  | / |
                         *  1 - 3 <- we're here!
                         *  
                         *  => (0,1,2)   (2,1,3)
                         */

                        // 0 1 2
                        triangles.Add(vertexIndices[x - 1, y - 1]);
                        triangles.Add(vertexIndices[x - 1, y]);
                        triangles.Add(vertexIndices[x, y - 1]);

                        // 2 1 3
                        triangles.Add(vertexIndices[x, y - 1]);
                        triangles.Add(vertexIndices[x - 1, y]);
                        triangles.Add(vertexIndices[x, y]);
                    }
                }
            }


            // display mesh with generated vertices
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }

        private void RegenerateMesh()
        {
            // need a mesh to work with
            if (mesh == null)
            {
                GenerateMesh();
            }
            else
            {
                vertices.Clear();

                for (int x = 0; x < data.GetLength(0); x++)
                {
                    for (int y = 0; y < data.GetLength(1); y++)
                    {
                        vertices.Add(new Vector3(x, data[x, y], y));
                    }
                }

                mesh.vertices = vertices.ToArray();
                mesh.RecalculateNormals();
            }
        }
    }
}
