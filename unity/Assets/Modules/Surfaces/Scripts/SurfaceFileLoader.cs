using Assets.Modules.Core;
using System;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class SurfaceFileLoader : MonoBehaviour
    {
        public string StartupFilename = "default_displays.json";

        void OnEnable()
        {
            LoadFromFile(StartupFilename);
        }

        #region Serializing

        [Serializable]
        private class SavedSurface
        {
            public string Name;
            public Vector3[] SurfaceCorners;
        }

        [Serializable]
        private class SavedSurfaces
        {
            // unity serializing workaround
            public SavedSurface[] Surfaces;
        }

        public static void SaveToFile(string filename)
        {
            var displays = new SavedSurfaces();
            var calibratedSurfaces = SurfaceManager.GetAll();

            displays.Surfaces = new SavedSurface[calibratedSurfaces.Count];
            int counter = 0;

            foreach (var pair in calibratedSurfaces)
            {
                displays.Surfaces[counter] = new SavedSurface
                {
                    Name = pair.Key,
                    SurfaceCorners = new[] {
                        pair.Value.GetCornerPosition(Corner.TopLeft),
                        pair.Value.GetCornerPosition(Corner.BottomLeft),
                        pair.Value.GetCornerPosition(Corner.BottomRight),
                        pair.Value.GetCornerPosition(Corner.TopRight),
                    }
                };

                counter++;
            }

            FileUtility.SaveToFile(filename, JsonUtility.ToJson(displays));
        }

        public static void LoadFromFile(string filename)
        {
            var content = FileUtility.LoadFromFile(filename);
            var displays = JsonUtility.FromJson<SavedSurfaces>(content);

            foreach (var display in displays.Surfaces)
            {
                SurfaceManager.Set(display.Name, display.SurfaceCorners[0], display.SurfaceCorners[1], display.SurfaceCorners[2], display.SurfaceCorners[3]);
            }
        }

        #endregion
    }
}
