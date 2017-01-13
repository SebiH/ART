using System;

namespace Assets.Modules.SurfaceGraph
{
    [Serializable]
    public class GraphInfo
    {
        public int id = 0;
        public string dimX = null;
        public string dimY = null;

        public string color = "#FFFFFF";

        public int[] selectedData = new int[0];
        public bool isSelected = false;

        public float pos = 0;
        public int index = 0;
    }
}
