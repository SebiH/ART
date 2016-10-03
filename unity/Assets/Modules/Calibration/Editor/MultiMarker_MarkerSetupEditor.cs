using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    [CustomEditor(typeof(MultiMarker_MarkerSetup))]
    public class MultiMarker_MarkerSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as MultiMarker_MarkerSetup;

            if (script.CanSetMarker)
            {
                if (GUILayout.Button("Set Marker") && Application.isPlaying)
                {
                    script.StartSetMarker();
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
        }
    }
}
