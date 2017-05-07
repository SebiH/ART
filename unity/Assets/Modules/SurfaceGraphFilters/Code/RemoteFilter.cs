using System;

namespace Assets.Modules.SurfaceGraphFilters
{
    [Serializable]
    public class RemoteFilter
    {
        // Must match interactivedisplay/client/app/services/FilterProvider.service
        [Serializable]
        public enum Type
        {
            Categorical = 0,
            Metric = 1,
            Detail = 2
        }

        public Type type = Type.Detail;

        /*
         *  (Base) Filter
         */
        public int id = -1;
        public int origin = -1;
        public string boundDimensions = "";
        public bool isUserGenerated = true;
        public bool isSelected = false;
        public float[] path;

        /*
         *  Category Filter
         */
        public int category = -1;
        public string color = "";

        /*
         *  Metric Filter
         */
        public float[] range = null;
        public GradientStop[] gradient = null;

        /*
         *  Detail Filter
         */
        //public string color = ""; already defined by category
        public string useAxisColor = "n"; // 'x' | 'y' -> axis, 'n' => no
        public Mapping[] mappings = null;



        [Serializable]
        public struct GradientStop
        {
            public float stop;
            public string color;
        }

        [Serializable]
        public struct Mapping
        {
            public int value;
            public string name;
            public string color;
        }
    }
}
