using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class GraphManager : MonoBehaviour
    {
        public Graph GraphTemplate;
        public RemoteDataProvider DataProvider;

        private readonly List<Graph> _graphs = new List<Graph>();

        public delegate void GraphEventHandler(Graph graph);
        public event GraphEventHandler OnGraphAdded;
        public event GraphEventHandler OnGraphDeleted;

        void OnEnable()
        {

        }

        void OnDisable()
        {
            while (_graphs.Count > 0)
            {
                RemoveGraph(_graphs[0].Id);
            }
        }



        public Graph GetGraph(int id)
        {
            return _graphs.FirstOrDefault(g => g.Id == id);
        }

        public IEnumerable<Graph> GetAllGraphs()
        {
            return _graphs;
        }

        public bool HasGraph(int id)
        {
            return _graphs.Any(g => g.Id == id);
        }

        public Graph CreateGraph(int id)
        {
            if (HasGraph(id))
            {
                Debug.LogWarning("Tried to create graph with id " + id + " twice");
                return GetGraph(id);
            }

            var graph = SpawnGraph();
            graph.Id = id;
            graph.DataProvider = DataProvider;
            _graphs.Add(graph);

            if (OnGraphAdded != null)
            {
                OnGraphAdded(graph);
            }
            
            return graph;
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

        
        private Graph SpawnGraph()
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
