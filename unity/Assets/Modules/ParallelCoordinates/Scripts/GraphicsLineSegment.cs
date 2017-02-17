using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphicsLineSegment : LineSegment_Old
    {
        private GraphicsLineRenderer _renderer;
        private int _lineIndex = -1;

        private void OnEnable()
        {
            _renderer = UnityUtility.FindParent<GraphicsLineRenderer>(this);
        }

        public override void SetPositions(Vector3 start, Vector3 end)
        {
            // TODO: add line in OnEnable, manipulate position after
            _renderer = UnityUtility.FindParent<GraphicsLineRenderer>(this);
            //_lineIndex = _renderer.AddLine(start, end);
        }

        public override void SetStartAnimated(Vector3 start)
        {
            // TODO
        }

        public override void SetEndAnimated(Vector3 end)
        {
            // TODO
        }


        public override void SetColor(Color col)
        {
            //if (col == Color.gray)
            //{
            //    _renderer.SetLineColor(_lineIndex, new Color32(0, 0, 0, 0));
            //}
            //else
            //{
            //    _renderer.SetLineColor(_lineIndex, new Color32(0, 255, 0, 255));
            //}
        }

        public override void SetWidth(float width)
        {
            // TODO
        }
    }
}
