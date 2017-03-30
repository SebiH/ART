using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public abstract class Dimension
    {
        public float[] Data = new float[Globals.DataPointsCount];
        public float[] ScaledData = new float[Globals.DataPointsCount];
        public string DisplayName = "";

        public float DomainMin = 0f;
        public float DomainMax = 1f;

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
