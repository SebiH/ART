using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public static class FixedDisplays
    {
        private static readonly Dictionary<string, FixedDisplay> _calibratedDisplays = new Dictionary<string, FixedDisplay>();

        public static bool Has(string name)
        {
            return _calibratedDisplays.ContainsKey(name);
        }

        public static void Set(string name, Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
        {
            if (Has(name))
            {
                // overwrite existing instance
                _calibratedDisplays[name] = display;
            }
            else
            {
                _calibratedDisplays.Add(name, display);
            }
        }

        #region Serializing

        public static void SaveToFile(string relativeFilename)
        {

        }

        public static void LoadFromFile(string relativeFilename)
        {

        }

        #endregion
    }
}
