using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    // TODO1 replace linegenerator?
    public class GraphConnection : MonoBehaviour
    {
        private Graph _startGraph;
        private Graph _endGraph;

        void OnEnable()
        {

        }
        
        void OnDisable()
        {
            // unsubscribe from any events
            SetStartGraph(null);
            SetEndGraph(null);
        }

        void Update()
        {
            if (_startGraph && _endGraph)
            {
                var scale = Mathf.Abs(_endGraph.transform.localPosition.z - _startGraph.transform.localPosition.z);
                transform.localScale = new Vector3(1, 1, scale);
            }
        }


        public void SetStartGraph(Graph graph)
        {
            if (graph == _startGraph)
            {
                return;
            }

            if (_startGraph)
            {

            }
        }

        public void SetEndGraph(Graph graph)
        {
            if (_endGraph == graph)
            {
                return;
            }

            if (_endGraph)
            {

            }
        }
    }
}
