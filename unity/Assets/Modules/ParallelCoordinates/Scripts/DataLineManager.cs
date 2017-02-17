using Assets.Modules.Core;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class DataLineManager : MonoBehaviour
    {
        public static DataLineManager Instance { get; set; }

        private DataLine[] _lines = new DataLine[0];
        private int[] _filter = null;
        private Coroutine _filterRoutine = null;

        private void OnEnable()
        {
            Instance = this;
            SetMaxDataIndex(5000);
        }

        // minor optimisation to avoid extending the array
        public void SetMaxDataIndex(int max)
        {
            if (_lines.Length < max + 1)
            {
                _lines = new DataLine[max + 1];
            }
        }

        public DataLine GetLine(int index)
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

        public void SetFilter(int[] filter)
        {
            _filter = filter;

            if (_filterRoutine != null)
            {
                StopCoroutine(_filterRoutine);
            }

            _filterRoutine = StartCoroutine(SetFilterAsync());
        }

        private IEnumerator SetFilterAsync()
        {
            using (var wd = new WorkDistributor())
            {
                yield return new WaitForEndOfFrame();
                wd.TriggerUpdate();

                foreach (var line in _lines)
                {
                    if (line != null)
                    {
                        SetFilter(line);
                        wd.Deplete(line.SegmentCount);

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

        private void SetFilter(DataLine line)
        {
            var isHighlighted = (_filter == null) || _filter.Contains(line.DataIndex);
            line.SetHighlight(isHighlighted);
        }
    }
}
