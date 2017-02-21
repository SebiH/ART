using Assets.Modules.Graphs;
using UnityEditor;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [CustomEditor(typeof(Graph))]
    public class ChangeGraphDimension : Editor
    {
        private string _dimX = "";
        private string _dimY = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as Graph;

            GUILayout.Label("DimX");
            _dimX = GUILayout.TextField(_dimX);
            GUILayout.Label("DimY");
            _dimY = GUILayout.TextField(_dimY);

            if (GUILayout.Button("Set"))
            {
                script.SetData(_dimX, _dimY);
            }
        }
    }
}
