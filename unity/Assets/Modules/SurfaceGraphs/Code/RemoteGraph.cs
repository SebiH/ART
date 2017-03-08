using System;

namespace Assets.Modules.SurfaceGraphs
{
    [Serializable]
    public class RemoteGraph
    {
        public int id = 0;
        public string dimX = null;
        public string dimY = null;

        public string color = "#FFFFFF";

        public bool isSelected = false;
        public bool isFlipped = false;
        public bool isColored = false;
        public bool isNewlyCreated = false;

        public float pos = 0;
        public float width = 0;
    }
}
