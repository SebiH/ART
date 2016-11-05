using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    [CustomEditor(typeof(CalibrationParams))]
    class CalibrationLoaderEditor : Editor
    {
        private static string _filename = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as CalibrationParams;

            GUILayout.Label("CalibrationOffsets filename");
            _filename = GUILayout.TextField(_filename);

            if (GUILayout.Button("Load Calibration Offsets") && Application.isPlaying)
            {
                script.LoadFromFile(_filename);
            }

            if (GUILayout.Button("Save current Offsets") && Application.isPlaying)
            {
                script.SaveToFile(_filename);
            }
        }
    }
}
