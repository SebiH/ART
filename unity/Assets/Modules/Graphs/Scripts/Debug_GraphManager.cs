using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(GraphManager))]
    public class Debug_GraphManager : MonoBehaviour
    {
        public GameObject GraphTemplate;
        public int NumGraphs = 5;
        public float SpaceBetweenGraphs = 0.1f;

        private GraphManager _graphManager;

        private void OnEnable()
        {
            _graphManager = GetComponent<GraphManager>();

            for (var i = 0; i < NumGraphs; i++)
            {
                _graphManager.CreateGraph(i).SetData("x " + i, "y" + i);
            }
        }

        private void OnDisable()
        {
            for (var i = 0; i < NumGraphs; i++)
            {
                _graphManager.RemoveGraph(i);
            }
        }

        private void Update()
        {
            foreach (var graph in _graphManager.GetAllGraphs())
            {
                graph.transform.position = new Vector3(0, 0, graph.Id * SpaceBetweenGraphs);
            }
        }
    }
}
