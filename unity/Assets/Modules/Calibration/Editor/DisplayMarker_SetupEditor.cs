using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    [CustomEditor(typeof(DisplayMarker_SetupDisplay))]
    public class DisplayMarker_SetupEditor : Editor
    {
        private static string _calibrateMarkerId = "";
        private static string _filename = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as DisplayMarker_SetupDisplay;

            if (script.CanCalibrate)
            {
                if (GUILayout.Button("Set Marker") && Application.isPlaying)
                {
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
