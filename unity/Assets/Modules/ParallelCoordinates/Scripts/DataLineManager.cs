using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class DataLineManager : MonoBehaviour
    {
        public static DataLineManager Instance { get; set; }

        private DataLine[] _lines = new DataLine[0];
        private int[] _filter = null;

        // minor optimisation to avoid extending the array
        public void SetMaxDataIndex(int max)
        {
            _lines = new DataLine[max + 1];
        }

        public DataLine GetLine(int index)
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

        public void SetFilter(int[] filter)
        {
            _filter = filter;
            foreach (var line in _lines)
            {
                SetFilter(line);
            }
        }

        private void SetFilter(DataLine line)
        {
            if (line != null)
            {
                var isHighlighted = (_filter == null) || _filter.Contains(line.DataIndex);
                line.SetHighlight(isHighlighted);
            }
        }
    }
}
