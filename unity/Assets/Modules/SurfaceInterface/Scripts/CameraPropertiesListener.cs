using Assets.Modules.Surfaces;
using Assets.Modules.Vision;
using Assets.Modules.Vision.CameraSources;
using System;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface.Scripts
{
    [RequireComponent(typeof(OvrvisionCameraSource))]
    public class CameraPropertiesListener : MonoBehaviour
    {
        private Surface _surface;
        private OvrvisionCameraSource _camera;
        private bool _isInitialized = false;

        private void Update()
        {
            if (!_isInitialized && SurfaceManager.Instance.Has("Surface"))
            {
                // TODO: surface name? listen globally?
                _surface = SurfaceManager.Instance.Get("Surface");
                _surface.OnAction += OnAction;

                _camera = GetComponent<OvrvisionCameraSource>();
                _isInitialized = true;
            }
        }

        private void OnDisable()
        {
            if (_isInitialized)
            {
                _surface.OnAction -= OnAction;
            }
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
