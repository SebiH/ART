using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class SurfaceManager : MonoBehaviour
    {
        public static SurfaceManager Instance { get; private set; }

        public Surface SurfaceTemplate;

        private readonly Dictionary<string, Surface> _calibratedDisplays = new Dictionary<string, Surface>();

        void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {

        }

        public bool Has(string name)
        {
            return _calibratedDisplays.ContainsKey(name);
        }

        public Dictionary<string, Surface> GetAll()
        {
            return _calibratedDisplays;
        }

        public Surface Get(string name)
        {
            return _calibratedDisplays[name];
        }

        public Surface Set(string name, Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
        {
            var surface = Instantiate(SurfaceTemplate);

            surface.SetCornerPosition(Corner.TopLeft, topLeft);
            surface.SetCornerPosition(Corner.BottomLeft, bottomLeft);
            surface.SetCornerPosition(Corner.BottomRight, bottomRight);
            surface.SetCornerPosition(Corner.TopRight, topRight);

            if (Has(name))
            {
                // TODO (?) overwrite existing instance
                _calibratedDisplays[name] = surface;
            }
            else
            {
                _calibratedDisplays.Add(name, surface);
            }

            return surface;
        }

    }
}
