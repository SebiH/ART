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
        private static readonly String MODULE_CONTOUR = "Contour";
        private static readonly String MODULE_ARTOOLKIT = "ARToolkit";

        public static string ModuleToString(Module m)
        {
            switch (m)
            {
                case Module.RawImage:
                    return MODULE_RAW_IMAGE;

                case Module.RegionOfInterest:
                    return MODULE_ROI;

                case Module.Contours:
                    return MODULE_CONTOUR;

                case Module.ARToolkit:
                    return MODULE_ARTOOLKIT;

                default:
                    throw new Exception("Unknown module " + m.ToString());
            }
        }
    }
}
