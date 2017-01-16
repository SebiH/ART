using Assets.Modules.Graphs;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class GraphConnection : MonoBehaviour
    {
        private Graph _startGraph = null;
        public Graph StartGraph
        {
            get { return _startGraph; } 
            set { if (value != _startGraph) { SetStartGraph(value); } }
        }

        private Graph _endGraph = null;
        public Graph EndGraph
        {
            get { return _endGraph; } 
            set { if (value != _endGraph) { SetEndGraph(value); } }
        }

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


        private void SetStartGraph(Graph graph)
        {
            if (graph == _startGraph)
            {
                return;
            }

            if (_startGraph)
            {

            }
        }

        private void SetEndGraph(Graph graph)
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
