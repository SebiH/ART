using Assets.Modules.Surfaces;
using Assets.Modules.Vision;
using Assets.Modules.Vision.CameraSources;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface
{
    [RequireComponent(typeof(OvrvisionCameraSource), typeof(SaveOvrSettings), typeof(AutoLoadOvrSettings))]
    public class CameraPropertiesListener : MonoBehaviour
    {
        private OvrvisionCameraSource _camera;
        private CameraGap _gapController;

        public Renderer LeftEye;
        public Renderer RightEye;


        private void OnEnable()
        {
            RemoteSurfaceConnection.OnCommandReceived += OnAction;
            _camera = GetComponent<OvrvisionCameraSource>();
            _gapController = GetComponent<CameraGap>();

#if UNITY_EDITOR
            StartCoroutine(SendSettings());
#endif
        }

        private void OnDisable()
        {
            RemoteSurfaceConnection.OnCommandReceived -= OnAction;
        }

#if UNITY_EDITOR
        private IEnumerator SendSettings()
        {
            while (enabled)
            {
                var settings = new OvrSettings
                {
                    Gain = _camera.Gain,
                    Exposure = _camera.Exposure,
                    BLC = _camera.BLC,
                    CameraGap = _gapController.Gap,
                    AutoContrast = _camera.AutoContrast,
                    AutoContrastAutoGain = _camera.AutoContrastAutoGain,
                    AutoContrastClipPercent = _camera.AutoContrastClipHistPercent,
                    AutoContrastMax = _camera.AutoContrastMax,
                    GapAutoAdjust = _gapController.AutoAdjust
                };

                RemoteSurfaceConnection.SendCommand("surface", "debug-camera-properties", JsonUtility.ToJson(settings));

                yield return new WaitForSecondsRealtime(1f);
            }
        }
#endif


        private void OnAction(string cmd, string payload)
        {
            if (cmd == "camera-properties")
            {
                var props = JsonUtility.FromJson<CamProperties>(payload);
                _camera.Gain = props.gain;
                _camera.Exposure = props.exposure;
                _camera.BLC = props.blc;
                _camera.AutoContrast = props.autoContrast;
                _camera.AutoContrastAutoGain = props.autoContrastAutoGain;
                _camera.AutoContrastClipHistPercent = props.autoContrastClipPercent;
                _camera.AutoContrastMax = props.autoContrastMax;

                _gapController.Gap = props.cameraGap;
                _gapController.AutoAdjust = props.gapAutoAdjust;
            }

            if (cmd == "camera-expps")
            {
                try
                {
                    var exposurePerSecond = float.Parse(payload.Replace("\"", ""));
                    _camera.ExposurePerSec = exposurePerSecond;
                }
                catch (Exception e)
                {
                    Debug.LogError("Could not set exposure per sec: " + e.Message);
                }
            }

            if (cmd == "camera-active")
            {
                var status = bool.Parse(payload.Replace("\"", ""));
                LeftEye.enabled = status;
                RightEye.enabled = status;
            }

            if (cmd == "save-camera-settings")
            {
                GetComponent<SaveOvrSettings>().Save();
            }

            if (cmd == "load-camera-settings")
            {
                GetComponent<AutoLoadOvrSettings>().LoadSettings();
            }
        }


        [Serializable]
        private struct CamProperties
        {
            public int gain;
            public int exposure;
            public int blc;
            public float cameraGap;
            public bool autoContrast;
            public bool autoContrastAutoGain;
            public float autoContrastClipPercent;
            public float autoContrastMax;
            public bool gapAutoAdjust;
        }
    }
}
