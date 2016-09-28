using Assets.Modules.Core.Code;
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

        public static void SaveToFile(string relativeFilename)
        {
            var offsets = new Offsets
            {
                Rotation = OpenVrRotationOffset,
                Position = OptitrackToCameraOffset
            };

            var absoluteFilename = Paths.GetAbsolutePath(relativeFilename);

            File.WriteAllText(absoluteFilename, JsonUtility.ToJson(offsets));
            Debug.Log(String.Format("Saved to {0}", absoluteFilename));
        }

        public static void LoadFromFile(string relativeFilename)
        {
            var absoluteFilename = Paths.GetAbsolutePath(relativeFilename);
            Debug.Log(String.Format("Loading from {0}", absoluteFilename));
            var contents = File.ReadAllText(absoluteFilename);
            var offsets = JsonUtility.FromJson<Offsets>(contents);

            OpenVrRotationOffset = offsets.Rotation;
            OptitrackToCameraOffset = offsets.Position;
            Debug.Log(String.Format("Loaded CalibrationOffsets: Rotation: {0}   Position: {1}", offsets.Rotation.ToString(), offsets.Position.ToString()));
        }
    }
}
