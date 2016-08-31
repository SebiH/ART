using UnityEngine;

namespace Assets.Modules.Tracking
{
    public static class CalibrationOffset
    {
        public static bool IsInitialized = false;
        // ? public static DateTime LastInitialization;

        public static Quaternion SteamVrRotationOffset;
        public static Vector3 OptitrackToCameraOffset;
    }
}
