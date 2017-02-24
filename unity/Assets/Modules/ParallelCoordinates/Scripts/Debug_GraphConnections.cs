using Assets.Modules.Graphs;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class Debug_GraphConnections : MonoBehaviour
    {
        private void Start()
        {
            var graphManager = GetComponent<GraphManager>();
            var graphs = graphManager.GetAllGraphs().ToArray();

            var filter = new int[10];
            for (int i = 0; i < filter.Length; i++)
            {
                filter[i] = i;
            }
            DataLineManager.SetFilter(filter);
        }
    }
}
