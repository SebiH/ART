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

        public bool useColorX = false;
        public bool useColorY = false;

        public bool isFlipped = false;
        public bool isNewlyCreated = false;
        public bool isPickedUp = false;
        public bool isSelected = false;

        public float pos = 0;
        public float width = 0;
    }
}
