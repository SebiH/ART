using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Deprecated.Graph
{
    class FileDataProvider : DataProvider
    {
        private float[,] data;

        public void ReadData(string path)
        {
            List<List<float>> dataList = new List<List<float>>();

            // TODO: file not found?
            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var row = new List<float>();

                    foreach (var val in values)
                    {
                        row.Add(float.Parse(val));
                    }

                    dataList.Add(row);
                }
            }

            // assuming all rows have equal length
            data = new float[dataList.Count + 1, dataList[0].Count + 1];

            int i = 0;

            foreach (var row in dataList)
            {
                int j = 0;

                foreach (var entry in row)
                {
                    data[i, j] = entry;
                    j++;
                }

                i++;
            }
        }


        public override float[,] GetData()
        {
            return data;
        }

        public override Vector2[] GetDimData(string dimX, string dimY)
        {
            throw new NotImplementedException();
        }
    }
}
