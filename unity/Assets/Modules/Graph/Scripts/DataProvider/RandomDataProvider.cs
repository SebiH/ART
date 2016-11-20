using System;
using UnityEngine;

namespace Assets.Modules.Graph
{
    class RandomDataProvider : DataProvider
    {
        public int width = 10;
        public int height = 10;

        public override float[,] GetData()
        {
            var data = new float[width, height];

            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    data[x, y] = UnityEngine.Random.Range(0f, 5f);
                }
            }

            return data;
        }

        public override Dimension GetDataDimensions()
        {
            throw new NotImplementedException();
        }
    }
}
