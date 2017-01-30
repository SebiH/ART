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

        private void OnEnable()
        {
            RemoteSurfaceConnection.Instance.OnCommandReceived += OnAction;
            _camera = GetComponent<OvrvisionCameraSource>();
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
