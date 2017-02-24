using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class Debug_GraphConnections : MonoBehaviour
    {
        private void Start()
        {
            var filter = new int[10];
            for (int i = 0; i < filter.Length; i++)
            {
                filter[i] = i;
            }
            DataLineManager.SetFilter(filter);
        }
    }
}
