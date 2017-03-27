using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphTracker : MonoBehaviour
    {
        public GraphMetaData TrackedGraph;

        private void LateUpdate()
        {
            Track();
        }
        
        public void Track()
        {
            if (TrackedGraph)
            {
                transform.position = TrackedGraph.transform.position;
                transform.rotation = TrackedGraph.transform.rotation;
                transform.localScale = TrackedGraph.transform.localScale;
            }
        }
    }
}
