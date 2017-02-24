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

            SetPositions();
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
            //SetPositions();
        }

        private void SetPositions()
        {
            foreach (var graph in _graphManager.GetAllGraphs())
            {
                var pos = graph.Id * SpaceBetweenGraphs;
                graph.transform.localPosition = new Vector3(0, 0, pos);
                graph.Position = pos;
            }
        }
    }
}
