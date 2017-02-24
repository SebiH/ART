using System.Collections.Generic;

namespace Assets.Modules.Graphs
{
    public class CategoricalDimension : Dimension
    {
        public struct Mapping
        {
            public string Name;
            public float Value;
        }

        public List<Mapping> Mappings = new List<Mapping>();
    }
}
