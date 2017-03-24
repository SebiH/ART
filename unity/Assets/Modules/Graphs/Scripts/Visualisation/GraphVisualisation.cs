using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class GraphVisualisation : MonoBehaviour
    {
        private Graph _graph;

        public GraphLabel LabelX;
        public GraphLabel LabelY;
        public GraphDataField DataField;

        private void OnEnable()
        {
            _graph = UnityUtility.FindParent<Graph>(this);
        }

        private void Update()
        {
            LabelX.Text = (_graph.DimX == null) ? "" : _graph.DimX.DisplayName;
            LabelY.Text = (_graph.DimY == null) ? "" : _graph.DimY.DisplayName;
        }
    }
}
