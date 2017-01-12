using Assets.Modules.Core;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Deprecated.Graph
{
    public class CsvDataProvider : DataProvider
    {
        public string Filename;

        private string[] _columnNames;
        //private string[] _rowNames;
        private float[,] _values;

        void OnEnable()
        {
            var absolutePath = FileUtility.GetPath(Filename);

            var rowNames = new List<string>();
            var values = new List<float>();

            using (var reader = new StreamReader(absolutePath))
            {
                // populate column names
                var line = reader.ReadLine();
                _columnNames = line.Split(',');

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    var entries = line.Split(',');
                    rowNames.Add(entries[0]);

                    for (int i = 1; i < entries.Length; i++)
                    {
                        try
                        {
                            values.Add(float.Parse(entries[i]));
                        }
                        catch
                        {
                            Debug.Log("Could not parse " + entries[i]);
                            values.Add(0f);
                        }
                    }
                }
            }


            var totalColumns = _columnNames.Length - 1; // first column is used for row ID
            var totalRows = values.Count / totalColumns;
            _values = new float[totalRows, totalColumns];
            //_rowNames = rowNames.ToArray();

            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < totalColumns; col++)
                {
                    _values[row, col] = values[row * totalColumns + col];
                }
            }
        }

        public override float[,] GetData()
        {
            return _values;
        }

        public override Vector2[] GetDimData(string dimX, string dimY)
        {
            // TODO: wait for actual data format
            return new []{ Vector2.zero };
        }
    }
}
