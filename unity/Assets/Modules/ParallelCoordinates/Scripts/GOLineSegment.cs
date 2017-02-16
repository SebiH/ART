using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GOLineSegment : LineSegment
    {
        public override void SetColor(Color col)
        {
        }

        public override void SetEndAnimated(Vector3 end)
        {
        }

        public override void SetPositions(Vector3 start, Vector3 end)
        {
            transform.localRotation = Random.rotation;
        }

        public override void SetStartAnimated(Vector3 start)
        {
        }

        public override void SetWidth(float width)
        {
        }
    }
}
