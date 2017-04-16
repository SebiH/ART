using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Vision
{
    [CustomEditor(typeof(SaveOvrSettings))]
    public class SaveOvrSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as SaveOvrSettings;

            if (GUILayout.Button("Save Settings") && Application.isPlaying)
            {
                script.Save();
            }
        }
    }
}
