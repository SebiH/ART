using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Modules.Graphs
{
    public class MetricDimension : Dimension
    {
        private float _range = 1;
        public float[] PossibleTicks = new float[0];
        public bool IsTimeBased = false;
        public string TimeFormat = "HH:mm";

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
            if (_range == 0) { _range = 1; }
            for (var i = 0; i < Data.Length; i++)
            {
                if (Data[i].IsNull)
                    ScaledData[i] = -0.6f;
                else 
                    ScaledData[i] = Scale(Data[i].Value);
            }

            var ticks = new List<Mapping>();
            var format = (_range <= 10) ? "{0:0.0}" : "{0:0}";

            var minTick = PossibleTicks.Min();
            var maxTick = PossibleTicks.Max();

            foreach (var tick in PossibleTicks)
            {
                if (tick <= DomainMin && tick >= minTick)
                {
                    minTick = tick;
                }

                if (tick >= DomainMax && tick <= maxTick)
                {
                    maxTick = tick;
                }
            }


            for (var i = 0; i < PossibleTicks.Length; i++)
            {
                var tickNum = PossibleTicks[i];

                if (minTick <= tickNum && tickNum <= maxTick)
                {
                    var name = "";
                    if (IsTimeBased)
                    {
                        var date = new DateTime();
                        date = date.AddSeconds(tickNum);
                        name = date.ToString(TimeFormat);
                    }
                    else
                    {
                        name = string.Format(format, tickNum);
                    }

                    ticks.Add(new Mapping
                    {
                        Name = name,
                        Value = tickNum
                    });
                }
            }

            Ticks = ticks.ToArray();
        }

        public override float Scale(float val)
        {
            return (val - DomainMin) / _range - 0.5f;
        }
    }
}
