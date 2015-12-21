using UnityEngine;

namespace Assets.Code.DataProvider
{
    class RandomDataProvider : IDataProvider
    {
        public int width = 50;
        public int height = 50;

        public float[,] GetData()
        {
            var data = new float[width, height];

            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    data[x, y] = Random.Range(0f, 5f);
                }
            }

            return data;
        }
    }
}
