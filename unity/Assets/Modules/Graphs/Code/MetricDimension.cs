using System;
using System.Collections.Generic;

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
            _range = DomainMax - DomainMin;
            for (var i = 0; i < Data.Length; i++)
            {
                ScaledData[i] = Scale(Data[i]);
            }

            var ticks = new List<Mapping>();
            var format = (_range <= 10) ? "{0:0.0}" : "{0:0}";

            for (var i = 0; i < PossibleTicks.Length; i++)
            {
                var tickNum = PossibleTicks[i];

                if (DomainMin <= tickNum && tickNum <= DomainMax)
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
