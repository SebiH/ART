using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public static class SurfaceManager
    {
        private static readonly Dictionary<string, Surface> _calibratedDisplays = new Dictionary<string, Surface>();

        public static bool Has(string name)
        {
            return _calibratedDisplays.ContainsKey(name);
        }

        public static Dictionary<string, Surface> GetAll()
        {
            return _calibratedDisplays;
        }

        public static Surface Get(string name)
        {
            return _calibratedDisplays[name];
        }

        public static void Set(string name, Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
        {
            var display = new Surface(topLeft, bottomLeft, bottomRight, topRight);

            if (Has(name))
            {
                // TODO (?) overwrite existing instance
                _calibratedDisplays[name] = display;
            }
            else
            {
                _calibratedDisplays.Add(name, display);
            }
        }

    }
}
