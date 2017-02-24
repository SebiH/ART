using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(Graph))]
    public class Debug_GraphLayouter : MonoBehaviour
    {
        public int Id;
        public Color Color;
        public bool IsSelected;
        public bool IsNewlyCreated;
        public float Scale;
        public string DimX = "";
        public string DimY = "";
        public bool IsAnimating;
        public float Width;

        private Graph _graph;
        private Debug_GraphManager _graphManager;

        private void Start()
        {
            _graph = GetComponent<Graph>();
            _graphManager = UnityUtility.FindParent<Debug_GraphManager>(this);

            Id = _graph.Id;
            Color = _graph.Color;
            IsSelected = _graph.IsSelected;
            IsNewlyCreated = _graph.IsNewlyCreated;
            Scale = _graph.Scale;
            DimX = _graph.DimX == null ? "" : _graph.DimX.DisplayName;
            DimY = _graph.DimY == null ? "" : _graph.DimY.DisplayName;
            IsAnimating = _graph.IsAnimating;
            Width = _graph.Width;
        }

        void Update()
        {
            _graph.Id = Id;
            _graph.Color = Color;
            _graph.IsSelected = IsSelected;
            _graph.IsNewlyCreated = IsNewlyCreated;
            _graph.Scale = Scale;
            _graph.IsAnimating = IsAnimating;
            _graph.Width = Width;

            _graph.DimX =   string.IsNullOrEmpty(DimX) ? null : _graphManager.GetRandomData(DimX);
            _graph.DimY =   string.IsNullOrEmpty(DimY) ? null : _graphManager.GetRandomData(DimY);

            _graph.Position = _graph.transform.localPosition.z;
        }
    }
}
