using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Assets.Modules.Tracking;

namespace Assets.Modules.Calibration
{
    [CustomEditor(typeof(MultiMarker_PerformCalibration))]
    public class MultiMarkerCalibrationEditor : Editor
    {
        private static string _calibrationFilename = "";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as MultiMarker_PerformCalibration;

            if (!script.IsCalibrating)
            {
                if (GUILayout.Button("Perform Calibration") && Application.isPlaying)
                {
                    script.Calibrate();
                }
            }
            else
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, script.CalibrationProgress, "Calibrating...");
                GUILayout.Space(19);
                EditorGUILayout.EndVertical();
                Repaint();
            }


            if (GUILayout.Button("Switch to Calibrated Scene") && Application.isPlaying)
            {
                SceneManager.LoadScene("Modules/Calibration/Scenes/CalibratedTestScene");
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
