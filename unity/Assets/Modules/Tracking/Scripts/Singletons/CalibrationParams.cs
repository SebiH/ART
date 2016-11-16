using Assets.Modules.Core;
using System;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class CalibrationParams : MonoBehaviour
    {
        private static CalibrationParams Instance;
        public static float LastCalibrationTime { get; private set; }

        private static Quaternion _rotationOffset = Quaternion.identity;
        private static Quaternion _targetRotationOffset = Quaternion.identity;
        private static Quaternion _startRotationOffset = Quaternion.identity;
        public static Quaternion OpenVrRotationOffset
        {
            get { return _rotationOffset; }
            set
            {
                _startRotationOffset = _rotationOffset;
                _targetRotationOffset = value;
                UpdateCalibration();
            }
        }

        private static Vector3 _camOffset = Vector3.zero;
        private static Vector3 _targetCamOffset = Vector3.zero;
        private static Vector3 _startCamOffset = Vector3.zero;
        public static Vector3 OptitrackToCameraOffset
        {
            get { return _camOffset; }
            set
            {
                _startCamOffset = _camOffset;
                _targetCamOffset = value;
                UpdateCalibration();
            }
        }

        private static void UpdateCalibration()
        {
            LastCalibrationTime = Time.unscaledTime;
        }


        public string StartupFile = "";
        [Range(0.1f, 10f)]
        public float Smoothing = 5f;
        public bool OverrideCalibration = false;

        public Vector3 OverrideRotation;
        public Vector3 OverridePosition;

        void OnEnable()
        {
            Instance = this;

            if (StartupFile.Length > 0)
            {
                LoadFromFile(StartupFile);
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            if (OverrideCalibration)
            {
                OptitrackToCameraOffset = OverridePosition;
                OpenVrRotationOffset = Quaternion.Euler(OverrideRotation);
            }
            else
            {
                OverridePosition = OptitrackToCameraOffset;
                OverrideRotation = OpenVrRotationOffset.eulerAngles;
            }
#endif

            if ((_targetCamOffset - _camOffset).sqrMagnitude > 0.0001f)
            {
                var deltaTime = (Time.unscaledTime - LastCalibrationTime) * (1 / Smoothing);
                _camOffset = Vector3.Lerp(_startCamOffset, _targetCamOffset, deltaTime);
                _rotationOffset = Quaternion.Lerp(_startRotationOffset, _targetRotationOffset, deltaTime);
            }
        }


        #region Serializing

        [Serializable]
        private class Offsets
        {
            public Quaternion Rotation;
            public Vector3 Position;
        }

        public void SaveToFile(string filename)
        {
            var offsets = new Offsets
            {
                Rotation = OpenVrRotationOffset,
                Position = OptitrackToCameraOffset
            };

            FileUtility.SaveToFile(filename, JsonUtility.ToJson(offsets));
        }

        public void LoadFromFile(string filename)
        {
            var contents = FileUtility.LoadFromFile(filename);
            var offsets = JsonUtility.FromJson<Offsets>(contents);

            OpenVrRotationOffset = offsets.Rotation;
            OptitrackToCameraOffset = offsets.Position;
            Debug.Log(String.Format("Loaded CalibrationOffsets: Rotation: {0}   Position: {1}", offsets.Rotation.ToString(), offsets.Position.ToString()));
        }

        #endregion
    }
}
