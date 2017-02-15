using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public abstract class LineSegment : MonoBehaviour
    {
        public abstract void SetPositions(Vector3 start, Vector3 end);
        public abstract void SetStartAnimated(Vector3 start);

        public abstract void SetEndAnimated(Vector3 end);

        public abstract void SetColor(Color col);

        public abstract void SetWidth(float width);
    }
}
