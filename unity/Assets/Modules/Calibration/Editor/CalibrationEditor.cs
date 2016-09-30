using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Modules.Tracking.Editor
{
    [CustomEditor(typeof(PerformCalibration))]
    class CalibrationEditor : UnityEditor.Editor
    {
        private static string _calibrationFilename = "";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Perform Calibration") && Application.isPlaying)
            {
                var script = target as PerformCalibration;
                script.Calibrate();
            }

            if (GUILayout.Button("Switch to Calibrated Scene") && Application.isPlaying)
            {
                SceneManager.LoadScene("Scenes/CalibratedTestScene");
            }

            GUILayout.Label("CalibrationOffsets filename");
            _calibrationFilename = GUILayout.TextField(_calibrationFilename);
            if (GUILayout.Button("Save CalibrationOffsets") && Application.isPlaying)
            {
                CalibrationOffset.SaveToFile(_calibrationFilename);
            }

            if (GUILayout.Button("Load CalibrationOffsets") && Application.isPlaying)
            {
                CalibrationOffset.LoadFromFile(_calibrationFilename);
            }
        }
    }
}
