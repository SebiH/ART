using Assets.Modules.Core;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public static class DataLineManager
    {
        private static DataLine[] _lines = new DataLine[0];
        private static int[] _filter = null;
        private static int _filterRoutine = -1;

        static DataLineManager()
        {
            SetMaxDataIndex(1000);
        }

        // minor optimisation to avoid extending the array
        public static void SetMaxDataIndex(int max)
        {
            if (_lines.Length < max + 1)
            {
                _lines = new DataLine[max + 1];
            }
        }

        public static DataLine GetLine(int index)
        {
            // should not happen
            if (index >= _lines.Length)
            {
                var lines = new DataLine[index + 1];
                Array.Copy(_lines, lines, _lines.Length);
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
