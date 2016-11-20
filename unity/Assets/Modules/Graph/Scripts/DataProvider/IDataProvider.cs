using UnityEngine;

namespace Assets.Modules.Graph
{
    public abstract class DataProvider : MonoBehaviour
    {
        public struct Dimension
        {
            public int Rows;
            public int Columns;
        }

        public abstract float[,] GetData();
        public abstract Dimension GetDataDimensions();
    }
}
