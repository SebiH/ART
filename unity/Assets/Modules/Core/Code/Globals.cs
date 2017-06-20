using System.IO;
using UnityEngine;

namespace Assets.Modules.Core
{
    // TODO: make available via editor??
    public static class Globals
    {
        public static readonly string CustomDataPath = Path.Combine(Application.dataPath, "../data/");

        public static readonly string CalibrationSavefile = "calibration_autosave.json";
        public static readonly string OvrSettingsSavefile = "ovr_parameters.json";

        public static readonly string SurfaceServerIp = "192.168.178.86";
        public static readonly int SurfaceServerPort = 8835;
        public static readonly int SurfaceWebPort = 81;

        public static readonly string OptitrackServerIp = "127.0.0.1";
        public static readonly int OptitrackServerPort = 16000;
        public static readonly float OptitrackScaling = 0.8f;

        public static readonly string OptitrackCalibratorName = "CalibrationHelper";
        public static readonly string OptitrackHmdName = "HMD";

        public static readonly string DefaultSurfaceName = "Surface";

        // View distance after which lines/points are disabled for performance gains
        public static readonly float DataViewDistance = 3f;

        // for selection, etc.
        public const float NormalAnimationSpeed = 0.5f;
        public const float QuickAnimationSpeed = 0.3f;
        // for scrolling, smoothing out values from webapp
        public const float FastAnimationSpeed = 0.05f;


        public static readonly Color32 FilterActiveColor = new Color32(139, 195, 74, 255);

    }
}
