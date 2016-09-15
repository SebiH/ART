using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public static class CalibrationOffset
    {
        public static bool IsCalibrated = false;
        public static DateTime LastCalibration;

        public static Quaternion OpenVrRotationOffset;
        public static Vector3 OptitrackToCameraOffset;


        [Serializable]
        private class Offsets
        {
            public Quaternion Rotation;
            public Vector3 Position;
        }

        public static void SaveToFile(string filename)
        {
            var offsets = new Offsets
            {
                Rotation = OpenVrRotationOffset,
                Position = OptitrackToCameraOffset
            };

            File.WriteAllText(filename, JsonUtility.ToJson(offsets));
        }

        public static void LoadFromFile(string filename)
        {
            var contents = File.ReadAllText(filename);
            var offsets = JsonUtility.FromJson<Offsets>(contents);

            OpenVrRotationOffset = offsets.Rotation;
            OptitrackToCameraOffset = offsets.Position;
        }
    }
}
