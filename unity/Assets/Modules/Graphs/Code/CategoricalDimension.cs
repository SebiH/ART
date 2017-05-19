using System.Collections.Generic;

namespace Assets.Modules.Graphs
{
    public class CategoricalDimension : Dimension
    {
        private float _range = 1;

        public List<Mapping> Mappings = new List<Mapping>();

        public override void RebuildData()
        {
            if (Data == null)
            {
                return;
            }

            if (ScaledData == null || ScaledData.Length != Data.Length)
            {
                ScaledData = new float[Data.Length];
            }

            _range = DomainMax - DomainMin;
            for (var i = 0; i < Data.Length; i++)
            {
                ScaledData[i] = Scale(Data[i].Value);
            }

            Ticks = Mappings.ToArray();
        }

        public override float Scale(float val)
        {
            return (val + 1 - DomainMin) / (_range + 2) - 0.5f;
        }
    }
}
