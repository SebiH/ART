using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Graph
{
    class RandomDataProvider : DataProvider
    {
        public int DimSize = 10;
        public int Columns = 10;
        public int Rows = 10;

        private Dictionary<string, float[]> _dimensions = new Dictionary<string, float[]>();
        private float[,] _data;

        public override float[,] GetData()
        {
            if (_data == null)
            {
                _data = new float[Rows, Columns];

                for (int x = 0; x < _data.GetLength(0); x++)
                {
                    for (int y = 0; y < _data.GetLength(1); y++)
                    {
                        _data[x, y] = Random.Range(0f, 1f);

                        // make peaks more rare
                        if (Random.Range(0f, 1f) > 0.8)
                        {
                            _data[x, y] += Random.Range(0f, 1f);
                        }
                    }
                }
            }

            return _data;
        }

        public override Vector2[] GetDimData(string dimX, string dimY)
        {
            if (!_dimensions.ContainsKey(dimX))
            {
                GenerateDimension(dimX);
            }

            if (!_dimensions.ContainsKey(dimY))
            {
                GenerateDimension(dimY);
            }

            var x = _dimensions[dimX];
            var y = _dimensions[dimY];

            Debug.Assert(x.Length == y.Length);
            var dimData = new Vector2[x.Length];
            for (int i = 0; i < dimData.Length; i++)
            {
                dimData[i].x = x[i];
                dimData[i].y = y[i];
            }

            return dimData;
        }

        private void GenerateDimension(string dim)
        {
            var dummyData = new float[DimSize];

            for (int i = 0; i < dummyData.Length; i++)
            {
                dummyData[i] = Random.Range(-0.5f, 0.5f);
            }

            _dimensions[dim] = dummyData;
        }
    }
}
