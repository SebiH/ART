using Assets.Modules.Core;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
{
    public class CalibrationParams : MonoBehaviour
    {
        private static CalibrationParams Instance;
        public static float LastCalibrationTime { get; private set; }

        private static Quaternion _rotationOffset = Quaternion.identity;
        public static Quaternion OpenVrRotationOffset
        {
            get { return _rotationOffset; }
            set
            {
                _rotationOffset = value;
                UpdateCalibration();
            }
        }

        private static Vector3 _camOffset = Vector3.zero;
        public static Vector3 OptitrackToCameraOffset
        {
            get { return _camOffset; }
            set
            {
                _camOffset = value;
                UpdateCalibration();
            }
        }

        private static void UpdateCalibration()
        {
            LastCalibrationTime = Time.unscaledTime;
        }


        void OnEnable()
        {
            Instance = this;
        }


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
