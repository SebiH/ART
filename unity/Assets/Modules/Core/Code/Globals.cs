using System.IO;
using UnityEngine;

namespace Assets.Modules.Core
{
    // TODO: make available via editor??
    public static class Globals
    {
        public static readonly string CustomDataPath = Path.Combine(Application.dataPath, "../data/");

        public static readonly string CalibrationSavefile = "calibration_autosave.json";

        public static readonly string SurfaceServerIp = "127.0.0.1";
        public static readonly int SurfaceServerPort = 8835;
        public static readonly int SurfaceWebPort = 81;

        public static readonly string OptitrackServerIp = "127.0.0.1";
        public static readonly int OptitrackServerPort = 16000;
        public static readonly float OptitrackScaling = 0.8f;

        public static readonly string OptitrackCalibratorName = "CalibrationHelper";
        public static readonly string OptitrackHmdName = "HMD";

        public static readonly string DefaultSurfaceName = "Surface";

        public const int DataPointsCount = 1000;


        // for selection, etc.
        public const float NormalAnimationSpeed = 0.6f;
        public const float QuickAnimationSpeed = 0.3f;
        // for scrolling, smoothing out values from webapp
        public const float FastAnimationSpeed = 0.05f;

    }
}
