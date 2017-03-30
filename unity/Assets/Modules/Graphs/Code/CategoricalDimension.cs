using System;
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

        public override void RebuildData()
        {
            for (var i = 0; i < Data.Length; i++)
            {
                ScaledData[i] = Scale(Data[i]);
            }
        }

        public override float Scale(float val)
        {
            return val;
        }
    }
}
