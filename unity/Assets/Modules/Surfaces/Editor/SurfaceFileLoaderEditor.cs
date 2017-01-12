using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    [CustomEditor(typeof(SurfaceFileLoader))]
    public class SurfaceFileLoaderEditor : Editor
    {
        private static string _filename = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Label("Filename");
            _filename = GUILayout.TextField(_filename);
            if (GUILayout.Button("Dump surfaces to file") && Application.isPlaying)
            {
                SurfaceFileLoader.SaveToFile(_filename);
            }

            if (GUILayout.Button("Load displays from file") && Application.isPlaying)
            {
                SurfaceFileLoader.LoadFromFile(_filename);
            }
        }
    }
}
