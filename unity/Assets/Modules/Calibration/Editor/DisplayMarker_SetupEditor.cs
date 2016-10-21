using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    [CustomEditor(typeof(MultiMarker_MarkerSetup))]
    public class DisplayMarker_SetupEditor : Editor
    {
        private static string _calibrateMarkerId = "";
        private static string _filename = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as DisplayMarker_SetupDisplay;

            if (script.CalibrationProgress < Mathf.Epsilon)
            {
                if (GUILayout.Button("Set Marker") && Application.isPlaying)
                {
                    int id = int.Parse(_calibrateMarkerId);
                    script.StartCalibration();
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


            //GUILayout.Label("CalibrationOffsets filename");
            //_filename = GUILayout.TextField(_filename);
            //if (GUILayout.Button("Dump Markers to file") && Application.isPlaying)
            //{
            //    script.SaveCalibratedMarkers(_filename);
            //}

            //if (GUILayout.Button("Load Markerdump") && Application.isPlaying)
            //{
            //    script.LoadCalibratedMarkers(_filename);
            //}
        }
    }
}
