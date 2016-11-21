using Assets.Modules.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Heatmap
{
    public class HeatmapGeneratorSM : MonoBehaviour
    {
        private HeatmapModelSM _model;
        private DataProvider _dataProvider;

        void OnEnable()
        {
            _model = GetComponent<HeatmapModelSM>();
            _dataProvider = GetComponent<DataProvider>();
            Generate();
        }


        public void Generate()
        {
            var data = _dataProvider.GetData();
            var dim = _dataProvider.GetDataDimensions();

            var vertices = new Vector3[dim.Columns * dim.Rows];
            var triangles = new int[(dim.Columns - 1) * (dim.Rows - 1) * 6];
            int[,] vertexIndices = new int[dim.Columns, dim.Rows];

            int vertexIndicesCounter = 0;
            int triangleCounter = 0;

            for (int col = 0; col < dim.Columns; col++)
            {
                for (int row = 0; row < dim.Rows; row++)
                {
                    var position = new Vector3(col, Random.Range(-1f, 2f), row);
                    vertices[vertexIndicesCounter] = position;

                    vertexIndices[col, row] = vertexIndicesCounter;
                    vertexIndicesCounter++;

                    bool isFirstRow = (row == 0);
                    bool isFirstColumn = (col == 0);

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
                        triangles[triangleCounter++] = vertexIndices[col - 1, row - 1];
                        triangles[triangleCounter++] = vertexIndices[col - 1, row];
                        triangles[triangleCounter++] = vertexIndices[col, row - 1];

                        // 2 1 3
                        triangles[triangleCounter++] = vertexIndices[col, row - 1];
                        triangles[triangleCounter++] = vertexIndices[col - 1, row];
                        triangles[triangleCounter++] = vertexIndices[col, row];
                    }
                }
            }

            _model.SetMesh(ref vertices, ref triangles);
        }
    }
}
