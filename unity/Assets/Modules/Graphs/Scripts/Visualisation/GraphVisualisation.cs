using Assets.Modules.Core;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    public class GraphVisualisation : MonoBehaviour
    {
        private Graph _graph;

        public GraphLabel LabelX;
        public GraphLabel LabelY;
        public GraphTicks TicksX;
        public GraphTicks TicksY;
        public GraphDataField DataField;
        public MeshRenderer BasePlane;

        private void OnEnable()
        {
            _graph = UnityUtility.FindParent<Graph>(this);
            _graph.OnDataChange += OnDataChange;
        }

        private void OnDisable()
        {
            _graph.OnDataChange -= OnDataChange;
        }

        private void Update()
        {
            if ((transform.position - SceneCameraTracker.Instance.transform.position).magnitude > Globals.DataViewDistance)
            {
                TicksX.IsVisible = false;
                TicksY.IsVisible = false;
            }
            else
            {
                TicksX.IsVisible = true;
                TicksY.IsVisible = true;
            }

            if ((transform.position - SceneCameraTracker.Instance.transform.position).magnitude > Globals.DataViewDistance * 2)
            {
                LabelX.IsVisible = false;
                LabelY.IsVisible = false;
            }
            else
            {
                LabelX.IsVisible = true;
                LabelY.IsVisible = true;
            }
        }

        private void OnDataChange()
        {
            LabelX.Text = (_graph.DimX == null) ? "" : _graph.DimX.DisplayName;
            LabelY.Text = (_graph.DimY == null) ? "" : _graph.DimY.DisplayName;

            TicksX.SourceDimension = _graph.DimX;
            TicksY.SourceDimension = _graph.DimY;
        }
    }
}
