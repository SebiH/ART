using Assets.Code.DataProvider;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Heatmap
{
    public class HeatmapGenerator : MonoBehaviour
    {
        private HeatmapModel _model;
        private IDataProvider _dataProvider;

        void OnEnable()
        {
            _model = GetComponent<HeatmapModel>();
            _dataProvider = new RandomDataProvider(); //GetComponent<IDataProvider>();
            Generate();
        }


        public void Generate()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            int[,] vertexIndices = new int[50, 50];

            int vertexIndicesCounter = 0;


            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    var position = new Vector3(x, Random.Range(-1f, 2f), y);
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
                         *  1 - 3 <- current position
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

            _model.SetMesh(vertices.ToArray(), triangles.ToArray());
        }
    }
}
