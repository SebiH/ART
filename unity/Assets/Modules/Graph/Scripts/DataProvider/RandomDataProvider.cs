using System;
using UnityEngine;

namespace Assets.Modules.Graph
{
    class RandomDataProvider : DataProvider
    {
        public int Columns = 10;
        public int Rows = 10;

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
                        _data[x, y] = UnityEngine.Random.Range(0f, 1f);
                    }
                }
            }

            return _data;
        }
    }
}
