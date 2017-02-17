using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(LineRenderer))]
    public class LRLineSegment : LineSegment_Old
    {
        private LineRenderer _lineRenderer;

        void OnEnable()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.numPositions = 2;
        }

        public override void SetPositions(Vector3 start, Vector3 end)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }

        public override void SetStartAnimated(Vector3 start)
        {
            // TODO: animate
            _lineRenderer.SetPosition(0, start);
        }

        public override void SetEndAnimated(Vector3 end)
        {
            // TODO: animate
            _lineRenderer.SetPosition(1, end);
        }


        public override void SetColor(Color col)
        {
            _lineRenderer.material.color = col;
        }

        public override void SetWidth(float width)
        {
            // TODO: animate
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
        }
    }
}
