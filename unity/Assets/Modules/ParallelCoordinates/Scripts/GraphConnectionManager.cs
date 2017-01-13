using Assets.Modules.Graphs;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    [RequireComponent(typeof(GraphManager))]
    public class GraphConnectionManager : MonoBehaviour
    {
        private GraphManager _graphManager;
        private readonly List<GraphConnection> _connections = new List<GraphConnection>();

        void OnEnable()
        {
            _graphManager = GetComponent<GraphManager>();
        }

        void OnDisable()
        {
        }


        public void SetConnection(int startId, int endId)
        {

        }

        public void RemoveConnection(int graphId)
        {

        }
    }
}
