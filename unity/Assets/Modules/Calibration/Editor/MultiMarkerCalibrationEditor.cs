using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Assets.Modules.Tracking;

namespace Assets.Modules.Calibration
{
    //[CustomEditor(typeof(PerformCalibrationMultiMarker))]
    public class MultiMarkerCalibrationEditor : Editor
    {
        private static string _calibrationFilename = "";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Perform Calibration") && Application.isPlaying)
            {
                var script = target as PerformCalibrationSingleMarker;
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
