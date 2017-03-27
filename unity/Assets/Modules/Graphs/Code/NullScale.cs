using System;

namespace Assets.Modules.Graphs
{
    public class NullScale : Scale
    {
        public override float Convert(float data)
        {
            return float.NaN;
        }
    }
}
