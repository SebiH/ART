using System;

namespace Assets.Modules.SurfaceGraphFilters
{
    [Serializable]
    public class RemoteFilter
    {
        public int id = -1;
        public int origin;
        public string color;
        public bool isOverview;

        public int type;
        public float[] path;
        public int category;
        public float[] range;
    }
}
