using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class GraphManager : MonoBehaviour
    {
        public GraphMetaData GraphTemplate;
        private readonly List<GraphMetaData> _graphs = new List<GraphMetaData>();

        public delegate void GraphEventHandler(GraphMetaData graph);
        public event GraphEventHandler OnGraphAdded;
        public event GraphEventHandler OnGraphDeleted;

        void OnDisable()
        {
            while (_graphs.Count > 0)
            {
                RemoveGraph(_graphs[0].Graph.Id);
            }
        }

        public GraphMetaData GetGraph(int id)
        {
            return _graphs.FirstOrDefault(g => g.Graph.Id == id);
        }

        public IEnumerable<GraphMetaData> GetAllGraphs()
        {
            return _graphs;
        }

        public bool HasGraph(int id)
        {
            return _graphs.Any(g => g.Graph.Id == id);
        }

        public GraphMetaData CreateGraph(int id)
        {
            if (HasGraph(id))
            {
                Debug.LogWarning("Tried to create graph with id " + id + " twice");
                return GetGraph(id);
            }

            var graphData = SpawnGraph();
            graphData.Graph.Id = id;
            _graphs.Add(graphData);

            if (OnGraphAdded != null)
            {
                OnGraphAdded(graphData);
            }

            return graphData;
        }

        public void RemoveGraph(int id)
        {
            var graph = GetGraph(id);
            if (graph)
            {
                _graphs.Remove(graph);

                if (OnGraphDeleted != null)
                {
                    OnGraphDeleted(graph);
                }

                Destroy(graph.gameObject);
            }
        }

        
        private GraphMetaData SpawnGraph()
        {
            var graph = Instantiate(GraphTemplate);
            graph.transform.parent = transform;
            graph.transform.localPosition = Vector3.zero;
            graph.transform.localRotation = Quaternion.identity;
            graph.transform.localScale = Vector3.one;

            return graph;
        }
    }
}
