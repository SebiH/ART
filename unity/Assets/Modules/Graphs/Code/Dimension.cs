namespace Assets.Modules.Graphs
{
    public abstract class Dimension
    {
        public float[] Data = new float[0];
        public string DisplayName = "";

        public float DomainMin = 0f;
        public float DomainMax = 1f;
    }
}
