using System;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public static class CalibrationOffset
    {
        public static bool IsCalibrated = false;
        public static DateTime LastCalibration;

        public static Quaternion OpenVrRotationOffset;
        public static Vector3 OptitrackToCameraOffset;
    }
}
