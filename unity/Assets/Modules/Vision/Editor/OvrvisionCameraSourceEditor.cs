using Assets.Modules.Vision.CameraSources;
using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Vision
{
    [CustomEditor(typeof(OvrvisionCameraSource))]
    public class OvrvisionCameraSourceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as OvrvisionCameraSource;

            if (Application.isPlaying && script.isActiveAndEnabled)
            {
                if (GUILayout.Button("1/60"))
                {
                    script.ExposurePerSec = 60f;
                }
                if (GUILayout.Button("1/120"))
                {
                    script.ExposurePerSec = 120f;
                }
                if (GUILayout.Button("1/180"))
                {
                    script.ExposurePerSec = 180f;
                }
                if (GUILayout.Button("1/240"))
                {
                    script.ExposurePerSec = 240f;
                }
            }

        }
    }
}
