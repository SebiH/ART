using Assets.Modules.Graph;
using UnityEngine;

namespace Assets.Modules.Heatmap
{
    public class HeatmapModelDynamic : MonoBehaviour
    {
        public DataProvider Provider;
        public GameObject DataPointTemplate;
        public Transform ModelContainer;

        private GameObject[,] _dataPoints;

        public GameObject GenerateDataPoint(int dataRow, int dataColumn)
        {
            var data = Provider.GetData();

            var maxRow = data.GetLength(0) - 1;
            var maxCol = data.GetLength(1) - 1;

            var instance = Instantiate(DataPointTemplate);
            instance.transform.parent = ModelContainer;

            var dataPoint = instance.GetComponent<HeatmapDataPoint>();

            var topLeft = data[dataRow, dataColumn];
            var bottomLeft = data[Mathf.Min(dataRow + 1, maxRow), dataColumn];
            var bottomRight = data[Mathf.Min(dataRow + 1, maxRow), Mathf.Min(dataColumn + 1, maxCol)];
            var topRight = data[dataRow, Mathf.Min(dataColumn + 1, maxCol)];
            dataPoint.SetData(topLeft, bottomLeft, bottomRight, topRight);

            dataPoint.RowIndex = dataRow;
            dataPoint.ColumnIndex = dataColumn;

            instance.transform.localPosition = new Vector3(dataColumn, 0, -dataRow);
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;

            return instance;
        }
    }
}
