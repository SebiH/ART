using UnityEditor;
using UnityEngine;


namespace Assets.Modules.Tracking
{
    namespace Assets.Modules.Calibration
    {
        [CustomEditor(typeof(OptitrackCalibrateDisplay))]
        public class OptitrackCalibrateDisplayEditor : Editor
        {
            private static string _calibrateMarkerId = "";
            private static string _filename = "";

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var script = target as OptitrackCalibrateDisplay;

                if (!script.IsCalibrating)
                {
                    if (GUILayout.Button("Set Corner") && Application.isPlaying)
                    {
                        script.StartCalibration();
                    }

                    if (GUILayout.Button("Commit display") && Application.isPlaying)
                    {
                        script.CommitFixedDisplay();
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


                GUILayout.Label("CalibrationOffsets filename");
                _filename = GUILayout.TextField(_filename);
                if (GUILayout.Button("Dump displays to file") && Application.isPlaying)
                {
                    FixedDisplays.SaveToFile(_filename);
                }

                if (GUILayout.Button("Load displays from file") && Application.isPlaying)
                {
                    FixedDisplays.LoadFromFile(_filename);
                }
            }
        }
    }

}
