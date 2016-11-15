using UnityEditor;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    [CustomEditor(typeof(InteractiveSurfaceClient))]
    public class InteractiveSurfaceClientEditor : Editor
    {
        private static string _cmdName = "";
        private static string _cmdTarget = "";
        private static string _cmdPayload = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as InteractiveSurfaceClient;

            GUILayout.Label("Command");
            _cmdName = GUILayout.TextField(_cmdName);

            GUILayout.Label("Target");
            _cmdTarget = GUILayout.TextField(_cmdTarget);

            GUILayout.Label("Payload");
            _cmdPayload = GUILayout.TextField(_cmdPayload);

            if (GUILayout.Button("Send") && Application.isPlaying)
            {
                script.SendCommand(new WebCommand
                {
                    command = _cmdName,
                    target = _cmdTarget,
                    payload = _cmdPayload
                });
            }
        }
    }
}
