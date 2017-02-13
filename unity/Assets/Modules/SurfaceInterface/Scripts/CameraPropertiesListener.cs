using Assets.Modules.Surfaces;
using Assets.Modules.Vision.CameraSources;
using System;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface.Scripts
{
    [RequireComponent(typeof(OvrvisionCameraSource))]
    public class CameraPropertiesListener : MonoBehaviour
    {
        private OvrvisionCameraSource _camera;

        public Transform LeftEye;
        private Vector3 _originalLeftEyePosition;

        public Transform RightEye;
        private Vector3 _originalRightEyePosition;


        private void OnEnable()
        {
            RemoteSurfaceConnection.Instance.OnCommandReceived += OnAction;
            _camera = GetComponent<OvrvisionCameraSource>();

            if (LeftEye) { _originalLeftEyePosition = LeftEye.localPosition; }
            if (RightEye) { _originalRightEyePosition = RightEye.localPosition; }
        }

        private void OnDisable()
        {
            RemoteSurfaceConnection.Instance.OnCommandReceived -= OnAction;
        }


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
