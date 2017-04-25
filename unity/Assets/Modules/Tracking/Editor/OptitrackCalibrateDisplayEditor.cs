using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    [CustomEditor(typeof(OptitrackCalibrateDisplay))]
    public class OptitrackCalibrateDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Repaint();
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
        }
    }
}
