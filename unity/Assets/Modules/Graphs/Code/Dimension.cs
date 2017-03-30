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

        protected static float[] RandomOffset = null;
        static Dimension()
        {
            RandomOffset = new float[Globals.DataPointsCount];
            for (var i = 0; i < RandomOffset.Length; i++)
            {
                RandomOffset[i] = Random.value / 50f;
            }
        }
    }
}
