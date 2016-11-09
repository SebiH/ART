using Assets.Modules.Core;
using System;
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

        public static FixedDisplay Get(string name)
        {
            return _calibratedDisplays[name];
        }

        public static void Set(string name, Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight, Resolution resolution)
        {
            var display = new FixedDisplay(topLeft, bottomLeft, bottomRight, topRight, resolution);

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

        #region Serializing

        [Serializable]
        private class SavedDisplay
        {
            public string Name;
            public FixedDisplay Display;
        }

        [Serializable]
        private class SavedDisplays
        {
            // unity serializing workaround
            public SavedDisplay[] Displays;
        }

        public static void SaveToFile(string filename)
        {
            var displays = new SavedDisplays();
            displays.Displays = new SavedDisplay[_calibratedDisplays.Count];
            int counter = 0;

            foreach (var pair in _calibratedDisplays)
            {
                displays.Displays[counter] = new SavedDisplay
                {
                    Name = pair.Key,
                    Display = pair.Value
                };

                counter++;
            }

            FileUtility.SaveToFile(filename, JsonUtility.ToJson(displays));
        }

        public static void LoadFromFile(string filename)
        {
            var content = FileUtility.LoadFromFile(filename);
            var displays = JsonUtility.FromJson<SavedDisplays>(content);

            foreach (var display in displays.Displays)
            {
                _calibratedDisplays.Add(display.Name, display.Display);
            }
        }

        #endregion
    }
}
