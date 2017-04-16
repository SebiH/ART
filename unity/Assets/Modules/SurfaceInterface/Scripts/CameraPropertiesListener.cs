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

        public Transform LeftEye;
        private Vector3 _originalLeftEyePosition;

        public Transform RightEye;
        private Vector3 _originalRightEyePosition;


        private void OnEnable()
        {
            RemoteSurfaceConnection.OnCommandReceived += OnAction;
            _camera = GetComponent<OvrvisionCameraSource>();

            if (LeftEye) { _originalLeftEyePosition = LeftEye.localPosition; }
            if (RightEye) { _originalRightEyePosition = RightEye.localPosition; }

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
                    BLC = _camera.BLC
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
            }

            if (cmd == "camera-gap")
            {
                var offset = float.Parse(payload.Replace("\"", ""));
                if (LeftEye) { LeftEye.localPosition = _originalLeftEyePosition + new Vector3(offset, 0, 0); }
                if (RightEye) { RightEye.localPosition = _originalRightEyePosition + new Vector3(-offset, 0, 0); }
            }

            if (cmd == "camera-active")
            {
                var status = bool.Parse(payload.Replace("\"", ""));
                if (LeftEye) { LeftEye.gameObject.GetComponent<Renderer>().enabled = status; }
                if (RightEye) { RightEye.GetComponent<Renderer>().enabled = status; }
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
        }
    }
}
