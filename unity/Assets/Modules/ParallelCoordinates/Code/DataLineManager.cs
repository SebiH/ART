namespace Assets.Modules.ParallelCoordinates
{
    public static class DataLineManager
    {
        private static DataLine[] _lines = new DataLine[100];

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
                _lines = new DataLine[index + 1];
            }

            if (_lines[index] == null)
            {
                _lines[index] = new DataLine(index);
            }

            return _lines[index];
        }
    }
}
