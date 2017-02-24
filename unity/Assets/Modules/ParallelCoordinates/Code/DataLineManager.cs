using Assets.Modules.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Modules.ParallelCoordinates
{
    public static class DataLineManager
    {
        private static List<DataLine> _lines = new List<DataLine>();
        private static int[] _filter = null;
        private static int _filterRoutine = -1;

        public static DataLine GetLine(int index)
        {
            while (index >= _lines.Count)
            {
                var line = new DataLine(_lines.Count);
                _lines.Add(line);
                SetFilter(line);
            }

            return _lines[index];
        }

        public static void SetFilter(int[] filter)
        {
            _filter = filter;

            if (_filterRoutine >= 0)
            {
                GameLoop.Instance.StopRoutine(_filterRoutine);
            }

            _filterRoutine = GameLoop.Instance.StartRoutine(SetFilterAsync());
        }

        private static IEnumerator SetFilterAsync()
        {
            const int BatchAmount = 50;

            var batchCounter = 0;
            foreach (var line in _lines)
            {
                if (line != null)
                {
                    if (batchCounter % BatchAmount == 0)
                    {
                        yield return new WaitForAvailableCycles(BatchAmount);
                    }

                    SetFilter(line);
                    batchCounter++;
                }
            }
        }

        private static void SetFilter(DataLine line)
        {
            var isFiltered = (_filter != null) && !(_filter.Contains(line.DataIndex));
            line.IsFiltered = isFiltered;
        }
    }
}
