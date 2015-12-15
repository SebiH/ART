using UnityEngine;
using System.Collections.Generic;

public class BarGraph : MonoBehaviour
{

    [Range(1, 100)]
    public int width;

    [Range(1, 100)]
    public int height;

    // data that will be visualised
    private float[,] data;

    // generated mesh
    private List<Vector3> vertices = new List<Vector3>();
    // TODO: dynamic?
    private int[,,] vertexIndices;
    private List<int> triangles = new List<int>();

    private Mesh mesh;

    // Use this for initialization
    void Start()
    {
        GenerateMesh();
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

            GenerateMesh();
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

        var indexCounter = 0;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                var height = data[x, y];
                var left = (x > 0) ? data[x - 1, y] : 0;
                var right = (x + 1 < columns) ? data[x + 1, y] : 0;
                var top = (y > 0) ? data[x, y - 1] : 0;
                var bottom = (y + 1 < rows) ? data[x, y + 1] : 0;

                /*
                 *   Top:
                 *   
                 *   0 - 1
                 *   | / |
                 *   2 - 3
                 *
                 */
                vertices.Add(new Vector3(x + 0.5f, height, y + 0.5f));
                vertices.Add(new Vector3(x - 0.5f, height, y + 0.5f));
                vertices.Add(new Vector3(x + 0.5f, height, y - 0.5f));
                vertices.Add(new Vector3(x - 0.5f, height, y - 0.5f));

                triangles.Add(indexCounter);
                triangles.Add(indexCounter + 2);
                triangles.Add(indexCounter + 1);

                triangles.Add(indexCounter + 1);
                triangles.Add(indexCounter + 2);
                triangles.Add(indexCounter + 3);

                indexCounter += 4;

                /*
                 *  Sides are only rendered by the bigger value!
                 */

                // left
                if (height > left)
                {
                    vertices.Add(new Vector3(x - 0.5f, height, y + 0.5f));
                    vertices.Add(new Vector3(x - 0.5f, left, y + 0.5f));
                    vertices.Add(new Vector3(x - 0.5f, height, y - 0.5f));
                    vertices.Add(new Vector3(x - 0.5f, left, y - 0.5f));

                    triangles.Add(indexCounter);
                    triangles.Add(indexCounter + 2);
                    triangles.Add(indexCounter + 1);

                    triangles.Add(indexCounter + 1);
                    triangles.Add(indexCounter + 2);
                    triangles.Add(indexCounter + 3);

                    indexCounter += 4;
                }

                // right
                if (height > right)
                {
                    vertices.Add(new Vector3(x + 0.5f, height, y + 0.5f));
                    vertices.Add(new Vector3(x + 0.5f, right, y + 0.5f));
                    vertices.Add(new Vector3(x + 0.5f, height, y - 0.5f));
                    vertices.Add(new Vector3(x + 0.5f, right, y - 0.5f));

                    triangles.Add(indexCounter);
                    triangles.Add(indexCounter + 1);
                    triangles.Add(indexCounter + 3);

                    triangles.Add(indexCounter);
                    triangles.Add(indexCounter + 3);
                    triangles.Add(indexCounter + 2);

                    indexCounter += 4;
                }

                // top
                if (height > top)
                {
                    vertices.Add(new Vector3(x + 0.5f, height, y - 0.5f));
                    vertices.Add(new Vector3(x + 0.5f, top, y - 0.5f));
                    vertices.Add(new Vector3(x - 0.5f, height, y - 0.5f));
                    vertices.Add(new Vector3(x - 0.5f, top, y - 0.5f));

                    triangles.Add(indexCounter);
                    triangles.Add(indexCounter + 1);
                    triangles.Add(indexCounter + 3);

                    triangles.Add(indexCounter);
                    triangles.Add(indexCounter + 3);
                    triangles.Add(indexCounter + 2);

                    indexCounter += 4;
                }

                // bottom
                if (height > bottom)
                {
                    vertices.Add(new Vector3(x + 0.5f, height, y + 0.5f));
                    vertices.Add(new Vector3(x + 0.5f, bottom, y + 0.5f));
                    vertices.Add(new Vector3(x - 0.5f, height, y + 0.5f));
                    vertices.Add(new Vector3(x - 0.5f, bottom, y + 0.5f));

                    triangles.Add(indexCounter);
                    triangles.Add(indexCounter + 3);
                    triangles.Add(indexCounter + 1);

                    triangles.Add(indexCounter);
                    triangles.Add(indexCounter + 2);
                    triangles.Add(indexCounter + 3);

                    indexCounter += 4;
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
}
