using UnityEngine;

namespace Assets.Modules.Graph
{
    public abstract class DataProvider : MonoBehaviour
    {
        public abstract float[,] GetData();

        // TODO: make abstract & proper implementations
        public Vector2[] GetDimData(string dimX, string dimY)
        {
            var randomData = new Vector2[100];
            for (int i = 0; i < randomData.Length; i++)
            {
                randomData[i] = Random.insideUnitCircle / 2f;
            }

            return randomData;
        }
    }
}
