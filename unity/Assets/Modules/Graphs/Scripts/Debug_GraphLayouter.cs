using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(GraphMetaData))]
    public class Debug_GraphLayouter : MonoBehaviour
    {
        public bool Apply = false;

        public int Id;
        public Color Color;
        public bool IsSelected;
        public bool IsNewlyCreated;
        public string DimX = "";
        public string DimY = "";
        public float Width;
        public float Position;

        private GraphMetaData _graph;
        private Debug_GraphManager _graphManager;

        private void Start()
        {
            _graph = GetComponent<GraphMetaData>();
            _graphManager = UnityUtility.FindParent<Debug_GraphManager>(this);

            FetchProperties();
        }

        void Update()
        {
            if (Apply)
            {
                ApplyProperties();
            }
            else
            {
                FetchProperties();
            }
        }

        private void FetchProperties()
        {
            Id = _graph.Graph.Id;
            Color = _graph.Graph.Color;
            IsSelected = _graph.Graph.IsSelected;
            IsNewlyCreated = _graph.Graph.IsNewlyCreated;
            DimX = _graph.Graph.DimX == null ? "" : _graph.Graph.DimX.DisplayName;
            DimY = _graph.Graph.DimY == null ? "" : _graph.Graph.DimY.DisplayName;
            Width = _graph.Layout.Width;
            Position = _graph.Layout.Position;
        }

        private void ApplyProperties()
        {
            _graph.Graph.Id = Id;
            _graph.Graph.Color = Color;
            _graph.Graph.IsSelected = IsSelected;
            _graph.Graph.IsNewlyCreated = IsNewlyCreated;

            if (_graphManager)
            {
                _graph.Graph.DimX = string.IsNullOrEmpty(DimX) ? null : _graphManager.GetRandomData(DimX);
                _graph.Graph.DimY = string.IsNullOrEmpty(DimY) ? null : _graphManager.GetRandomData(DimY);
            }

            _graph.Layout.Width = Width;
            _graph.Layout.Position = _graph.transform.localPosition.x;
        }
    }
}
