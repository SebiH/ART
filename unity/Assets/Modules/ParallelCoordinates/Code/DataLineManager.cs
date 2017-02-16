using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public static class DataLineManager
    {
        private static DataLine[] _lines = new DataLine[0];
        private static int[] _filter = null;

        // minor optimisation to avoid extending the array
        public static void SetMaxDataIndex(int max)
        {
            _lines = new DataLine[max + 1];
        }

        public static DataLine GetLine(int index)
        {
            // should not happen
            if (index >= _lines.Length)
            {
                var lines = new DataLine[index + 1];

                for (int i = 0; i < _lines.Length; i++)
                {
                    lines[i] = _lines[i];
                }

                _lines = lines;
            }

            if (_lines[index] == null)
            {
                var line = new DataLine(index);
                _lines[index] = line;
                SetFilter(line);
            }

            return _lines[index];
        }

        public static void SetFilter(int[] filter)
        {
            _filter = filter;
            foreach (var line in _lines)
            {
                SetFilter(line);
            }
        }

        private static void SetFilter(DataLine line)
        {
            if (line != null)
            {
                var isHighlighted = (_filter == null) || _filter.Contains(line.DataIndex);
                line.SetHighlight(isHighlighted);
            }
        }
    }
}
