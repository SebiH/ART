using Assets.Modules.Heatmap;
using UnityEngine;

namespace Assets.Modules.Graph
{
    public class DynamicDataLoader : DataLoader
    {
        public HeatmapModelDynamic Heatmap;

        // radius around which points are (un)loaded
        public int LoadRegionRadius = 1;

        private GameObject[,] _generatedObjects;

        void OnEnable()
        {
            var data = Heatmap.Provider.GetData();
            var rows = data.GetLength(0);
            var cols = data.GetLength(1);

            _generatedObjects = new GameObject[rows, cols];

            // TODO: start up by placing first datapoint in middle of hitbox
            var hr = 0;
            var hc = 0;

            var point = Heatmap.GenerateDataPoint(hr, hc);
            _generatedObjects[hr, hc] = point;
            LoadRegionAround(point.GetComponent<DataPoint>());
        }

        void OnDisable()
        {

        }

        void OnTriggerEnter(Collider col)
        {
            var dataPoint = col.GetComponent<DataPoint>();

            if (dataPoint != null)
            {
                LoadRegionAround(dataPoint);
            }
        }

        void OnTriggerExit(Collider col)
        {
            var dataPoint = col.GetComponent<DataPoint>();

            if (dataPoint != null)
            {
                UnloadRegionAround(dataPoint);
            }
        }


        private void LoadRegionAround(DataPoint p)
        {
            p.GetComponent<Renderer>().enabled = true;

            var maxRows = _generatedObjects.GetLength(0) - 1;
            var maxCols = _generatedObjects.GetLength(1) - 1;

            var startRow = Mathf.Max(p.RowIndex - LoadRegionRadius, 0);
            var endRow = Mathf.Min(p.RowIndex + LoadRegionRadius, maxRows);

            var startCol = Mathf.Max(p.ColumnIndex - LoadRegionRadius, 0);
            var endCol = Mathf.Min(p.ColumnIndex + LoadRegionRadius, maxCols);

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    if (row == p.RowIndex && col == p.ColumnIndex)
                    {
                        continue;
                    }

                    if (_generatedObjects[row, col] == null)
                    {
                        var newPoint = Heatmap.GenerateDataPoint(row, col);
                        newPoint.GetComponent<Renderer>().enabled = false;
                        _generatedObjects[row, col] = newPoint;
                    }
                }
            }
        }

        private void UnloadRegionAround(DataPoint p)
        {
            p.GetComponent<Renderer>().enabled = false;

            var maxRows = _generatedObjects.GetLength(0) - 1;
            var maxCols = _generatedObjects.GetLength(1) - 1;

            // Note: the surrounding datapoints should always be active, so that panning
            // always loads data if user pans model completely outside
            // -> max(1, ...); min(max - 1, ...) instead of 0
            var startRow = Mathf.Max(p.RowIndex - LoadRegionRadius, 1);
            var endRow = Mathf.Min(p.RowIndex + LoadRegionRadius, maxRows - 1);

            var startCol = Mathf.Max(p.ColumnIndex - LoadRegionRadius, 1);
            var endCol = Mathf.Min(p.ColumnIndex + LoadRegionRadius, maxCols - 1);

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    if (row == p.RowIndex && col == p.ColumnIndex)
                    {
                        continue;
                    }

                    var oldPoint = _generatedObjects[row, col];

                    if (oldPoint != null && !(oldPoint.GetComponent<Renderer>().isVisible))
                    {
                        _generatedObjects[row, col] = null;
                        Destroy(oldPoint);
                    }
                }
            }
        }
    }
}
