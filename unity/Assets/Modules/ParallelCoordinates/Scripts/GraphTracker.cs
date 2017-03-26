using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphTracker : MonoBehaviour
    {
        private Graph _trackedGraph;


        private void LateUpdate()
        {
            if (_trackedGraph)
            {
                transform.position = _trackedGraph.transform.position;
                transform.rotation = _trackedGraph.transform.rotation;
                transform.localScale = _trackedGraph.transform.localScale;
            }
        }
    }
}
