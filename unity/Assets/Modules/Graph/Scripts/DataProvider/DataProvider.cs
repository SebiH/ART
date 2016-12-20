using UnityEngine;

namespace Assets.Modules.Graph
{
    public abstract class DataProvider : MonoBehaviour
    {
        public abstract float[,] GetData();

        public abstract Vector2[] GetDimData(string dimX, string dimY);
    }
}
