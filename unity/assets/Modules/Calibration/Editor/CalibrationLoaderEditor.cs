using Assets.Modules.Tracking;
using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    [CustomEditor(typeof(CalibrationLoader))]
    class CalibrationLoaderEditor : Editor
    {
        private string _filename = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as CalibrationLoader;

            GUILayout.Label("CalibrationOffsets filename");
            _filename = GUILayout.TextField(_filename);

            if (GUILayout.Button("Load Calibration Offsets") && Application.isPlaying)
            {
                CalibrationOffset.LoadFromFile(_filename);
            }
        }
    }
}
