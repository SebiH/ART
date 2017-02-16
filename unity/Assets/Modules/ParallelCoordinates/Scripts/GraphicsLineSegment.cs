using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphicsLineSegment : LineSegment
    {
        public override void SetPositions(Vector3 start, Vector3 end)
        {
            // TODO
            var renderer = UnityUtility.FindParent<GraphicsLineRenderer>(this);
            renderer.AddLine(start, end);
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
            // TODO
        }

        public override void SetWidth(float width)
        {
            // TODO
        }
    }
}
