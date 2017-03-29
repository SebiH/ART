using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class GraphManager : MonoBehaviour
    {
        public GraphMetaData GraphTemplate;
        private readonly List<GraphMetaData> _graphs = new List<GraphMetaData>();
        private readonly List<GraphMetaData> _tempGraphs = new List<GraphMetaData>();

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
            var graph = _graphs.FirstOrDefault(g => g.Graph.Id == id);
            if (!graph)
            {
                return _tempGraphs.FirstOrDefault(g => g.Graph.Id == id);
            }
            return graph;
        }

        public IEnumerable<GraphMetaData> GetAllGraphs()
        {
            return _graphs;
        }

        public bool HasGraph(int id)
        {
            return _graphs.Any(g => g.Graph.Id == id) || _tempGraphs.Any(g => g.Graph.Id == id);
        }

        public GraphMetaData CreateGraph(int id)
        {
            if (HasGraph(id))
            {
                Debug.LogWarning("Tried to create graph with id " + id + " twice");
                return GetGraph(id);
            }

            var graphData = SpawnGraph(id);
            graphData.Graph.IsNewlyCreated = false;
            _graphs.Add(graphData);
            _tempGraphs.Remove(graphData);

            if (OnGraphAdded != null)
            {
                OnGraphAdded(graphData);
            }

            return graphData;
        }

        public void RegisterGraph(GraphMetaData graphData)
        {
            _graphs.Add(graphData);
            _tempGraphs.Remove(graphData);

            graphData.Graph.IsNewlyCreated = false;

            if (OnGraphAdded != null)
            {
                OnGraphAdded(graphData);
            }
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

        
        public GraphMetaData SpawnGraph(int id)
        {
            var graph = Instantiate(GraphTemplate);
            graph.Graph.IsNewlyCreated = true;
            graph.transform.parent = transform;
            graph.transform.localPosition = Vector3.zero;
            graph.transform.localRotation = Quaternion.identity;
            graph.transform.localScale = Vector3.one;

            graph.Graph.Id = id;
            _tempGraphs.Add(graph);

            return graph;
        }
    }
}
