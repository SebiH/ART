using System;
using UnityEngine;

namespace Assets.Modules.Graph
{
    class RandomDataProvider : DataProvider
    {
        public int Columns = 10;
        public int Rows = 10;

        public override float[,] GetData()
        {
            var data = new float[Rows, Columns];

            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    data[x, y] = UnityEngine.Random.Range(0f, 5f);
                }
            }

            return data;
        }
    }
}
