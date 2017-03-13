using System.Collections.Generic;

namespace Assets.Modules.ParallelCoordinates
{
    public static class DataLineManager
    {
        private static List<DataLine> _lines = new List<DataLine>();

        public static DataLine GetLine(int index)
        {
            while (index >= _lines.Count)
            {
                var line = new DataLine(_lines.Count);
                _lines.Add(line);
            }

            return _lines[index];
        }

        public static int MaxIndex()
        {
            return _lines.Count;
        }
    }
}
