using System.Collections.Generic;
using System.Linq;

namespace Assets.Modules.ParallelCoordinates
{
    public static class DataLineManager
    {
        private static DataLine[] _lines = new DataLine[0];

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
            }

            if (_lines[index] == null)
            {
                _lines[index] = new DataLine(index);
            }

            return _lines[index];
        }

        #region Filtering

        private static int[] _idFilter = null;

        public static void ClearFilter()
        {
            _idFilter = null;
        }

        public static void AddFilter(int[] remainingIds)
        {
            if (_idFilter == null)
            {
                _idFilter = remainingIds;
            }
            else
            {
                var newFilter = new List<int>();

                foreach (var id in remainingIds)
                {
                    if (_idFilter.Contains(id))
                    {
                        newFilter.Add(id);
                    }
                }

                _idFilter = newFilter.ToArray();
            }

        }

        public static void ApplyFilter()
        {
            foreach (var line in _lines)
            {
                var isHighlighted = (_idFilter == null) || _idFilter.Contains(line.DataIndex);
                line.SetHighlight(isHighlighted);
            }
        }

        #endregion
    }
}
