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
        private static Coroutine _filterRoutine = null;

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

            if (_filterRoutine != null)
            {
                GameLoop.Instance.StopRoutine(_filterRoutine);
            }

            _filterRoutine = GameLoop.Instance.StartRoutine(SetFilterAsync());
        }

        private static IEnumerator SetFilterAsync()
        {
            using (var wd = new WorkDistributor())
            {
                yield return new WaitForEndOfFrame();
                wd.TriggerUpdate();

                foreach (var line in _lines)
                {
                    if (line != null)
                    {
                        var hasChanged = SetFilter(line);
                        if (hasChanged)
                        {
                            wd.Deplete(line.SegmentCount / 4);
                        }

                        if (wd.AvailableCycles <= 0)
                        {
                            yield return new WaitForEndOfFrame();
                            wd.TriggerUpdate();
                        }
                    }
                }

                _filterRoutine = null;
            }
        }

        private static bool SetFilter(DataLine line)
        {
            var isFiltered = (_filter != null) && !(_filter.Contains(line.DataIndex));
            if (line.IsFiltered != isFiltered)
            {
                line.IsFiltered = isFiltered;
                return true;
            }
            return false;
        }
    }
}
