namespace Assets.Modules.Graphs
{
    public class MetricDimension : Dimension
    {
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
