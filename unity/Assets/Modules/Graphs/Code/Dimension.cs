using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public abstract class Dimension
    {
        public struct DimData
        {
            public int Id;
            public float Value;
        }

        public DimData[] Data = null;
        public float[] ScaledData = null;
        public string DisplayName = "";

        public float DomainMin = 0f;
        public float DomainMax = 1f;

        public bool HideTicks = false;

        public struct Mapping
        {
            public string Name;
            public float Value;
        }

        public Mapping[] Ticks;

        public abstract void RebuildData();
        public abstract float Scale(float val);
    }
}
