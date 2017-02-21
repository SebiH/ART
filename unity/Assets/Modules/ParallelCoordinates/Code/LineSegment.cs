using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineSegment
    {
        public Vector3 Start;
        public Vector3 End;

        public Color32 Color = new Color32(25, 118, 210, 255);
        public bool IsFiltered;


        public int MeshIndex = -1;

        private GraphicsLineRenderer _renderer;

        public void SetRenderer(GraphicsLineRenderer renderer)
        {
            Debug.Assert(_renderer == null, "Cannot reassign LineSegment to different renderer!");
            _renderer = renderer;
            _renderer.AddLine(this);
        }

        public void UpdateVisual()
        {
            if (MeshIndex >= 0)
            {
                _renderer.UpdateLine(this);
            }
        }
    }
}
