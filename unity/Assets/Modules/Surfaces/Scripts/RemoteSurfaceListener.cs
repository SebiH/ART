using System;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    [RequireComponent(typeof(Surface))]
    public class RemoteSurfaceListener : MonoBehaviour
    {
        private Surface _surface;

        void OnEnable()
        {
            _surface = GetComponent<Surface>();
            _surface.OnAction += OnSurfaceAction;
        }

        void OnDisable()
        {
            _surface.OnAction -= OnSurfaceAction;
        }

        private void OnSurfaceAction(string cmd, string payload)
        {
            if (cmd == "surface")
            {
                var properties = JsonUtility.FromJson<SurfaceActionPayload>(payload);

                var resolution = new Resolution
                {
                    width = properties.width,
                    height = properties.height
                };
                _surface.DisplayResolution = resolution;
                _surface.PixelToCmRatio = properties.pixelToCmRatio;
            }
        }

        [Serializable]
        private class SurfaceActionPayload
        {
            public float pixelToCmRatio = 1;
            public int width = 0;
            public int height = 0;
        }
    }
}
