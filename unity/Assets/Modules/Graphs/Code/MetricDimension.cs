namespace Assets.Modules.Graphs
{
    public class MetricDimension : Dimension
    {
        private float _range = 1;

        public override void RebuildData()
        {
            _range = DomainMax - DomainMin;
            for (var i = 0; i < Data.Length; i++)
            {
                ScaledData[i] = Scale(Data[i]);
            }
        }

        public override float Scale(float val)
        {
            return (val - DomainMin) / _range - 0.5f;
        }
    }
}
