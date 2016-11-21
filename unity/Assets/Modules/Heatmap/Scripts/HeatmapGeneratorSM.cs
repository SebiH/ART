using Assets.Modules.Graph;
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
            var rows = data.GetLength(0);
            var cols = data.GetLength(1);

            var vertices = new Vector3[cols * rows];
            var triangles = new int[(cols - 1) * (rows - 1) * 6];
            int[,] vertexIndices = new int[cols, rows];

            int vertexIndicesCounter = 0;
            int triangleCounter = 0;

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
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
