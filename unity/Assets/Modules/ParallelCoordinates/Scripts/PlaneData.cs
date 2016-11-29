using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class PlaneData : MonoBehaviour
    {
        public LineGenerator Generator;

        public void SetData(Vector2[] startData, Vector2[] endData)
        {
            Generator.GenerateLines(startData, endData);
        }
    }
}
