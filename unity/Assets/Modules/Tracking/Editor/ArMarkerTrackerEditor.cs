using UnityEditor;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    [CustomEditor(typeof(ArMarkerTracker), true)]
    public class ArMarkerTrackerEditor : Editor
    {
        private string _arMarkerSize = "10";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as ArMarkerTracker;

            GUILayout.Label("ArMarkerSize (in m?)");
            _arMarkerSize = GUILayout.TextField(_arMarkerSize);

            if (GUILayout.Button("Set Marker Size") && Application.isPlaying)
            {
                try
                {
                    var size = float.Parse(_arMarkerSize);
                    script.MarkerSizeInMeter = Mathf.Max(size, 0.0001f);
                }
                catch
                {
                    Debug.LogError("Unable to parse " + _arMarkerSize);
                }
            }
        }
    }
}
