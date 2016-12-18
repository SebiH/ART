using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public abstract class LineGenerator : MonoBehaviour
    {
        public abstract void GenerateLines(Vector2[] startData, Vector2[] endData);
        public abstract void SetStart(Vector2[] startData);
        public abstract void SetEnd(Vector2[] endData);
    }
}
