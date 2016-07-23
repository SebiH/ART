using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Vision
{
    public class ModuleUtils
    {
        private static readonly String MODULE_RAW_IMAGE = "RawImage";
        private static readonly String MODULE_ROI = "ROI";

        public static string ModuleToString(Module m)
        {
            switch (m)
            {
                case Module.RawImage:
                    return MODULE_RAW_IMAGE;

                case Module.RegionOfInterest:
                    return MODULE_ROI;

                default:
                    throw new Exception("Unknown module " + m.ToString());
            }
        }
    }
}
