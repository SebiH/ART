using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class LineSegment : MonoBehaviour
    {
        // for faster access
        private LineRenderer _lineRenderer;

        void OnEnable()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.numPositions = 2;
        }

        public void SetPositions(Vector3 start, Vector3 end)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }

        public void SetStartAnimated(Vector3 start)
        {
            // TODO: animate
            _lineRenderer.SetPosition(0, start);
        }

        public void SetEndAnimated(Vector3 end)
        {
            // TODO: animate
            _lineRenderer.SetPosition(1, end);
        }


        // best called from ContinualLine
        public void SetColor(Color col)
        {
            _lineRenderer.material.color = col;
        }
    }
}
