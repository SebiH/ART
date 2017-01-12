using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.ParallelCoordinates
{
    public static class ContinualLines
    {
        private static ContinualLine[] _lines = new ContinualLine[100];

        // minor optimisation to avoid extending the array
        public static void SetMaxDataIndex(int max)
        {
            _lines = new ContinualLine[max + 1];
        }

        public static ContinualLine Get(int index)
        {
            // should not happen
            if (index >= _lines.Length)
            {
                _lines = new ContinualLine[index + 1];
            }

            if (_lines[index] == null)
            {
                _lines[index] = new ContinualLine(index);
            }

            return _lines[index];
        }
    }
}
