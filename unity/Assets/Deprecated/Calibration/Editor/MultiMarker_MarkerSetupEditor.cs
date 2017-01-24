using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Calibration_Deprecated
{
    [CustomEditor(typeof(MultiMarker_MarkerSetup))]
    public class MultiMarker_MarkerSetupEditor : Editor
    {
        private static string _calibrateMarkerId = "";
        private static string _filename = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as MultiMarker_MarkerSetup;

            GUILayout.Label("Calibrate Marker ID:");
            _calibrateMarkerId = GUILayout.TextField(_calibrateMarkerId);

            if (script.CanSetMarker)
            {
                if (GUILayout.Button("Set Marker") && Application.isPlaying)
                {
                    int id = int.Parse(_calibrateMarkerId);
                    script.StartSetMarker(id);
                }
            }
            else
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, script.SetMarkerProgress, "Calibrating...");
                GUILayout.Space(19);
                EditorGUILayout.EndVertical();
                Repaint();
            }


            GUILayout.Label("CalibrationOffsets filename");
            _filename = GUILayout.TextField(_filename);
            if (GUILayout.Button("Dump Markers to file") && Application.isPlaying)
            {
                script.SaveCalibratedMarkers(_filename);
            }

            if (GUILayout.Button("Load Markerdump") && Application.isPlaying)
            {
                script.LoadCalibratedMarkers(_filename);
            }
        }
    }
}
