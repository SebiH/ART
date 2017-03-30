using Assets.Modules.Core;

namespace Assets.Modules.Graphs
{
    public abstract class Dimension
    {
        public float[] Data = new float[Globals.DataPointsCount];
        public float[] ScaledData = new float[Globals.DataPointsCount];
        public string DisplayName = "";

        public float DomainMin = 0f;
        public float DomainMax = 1f;

        public abstract void RebuildData();
        public abstract float Scale(float val);
    }
}
