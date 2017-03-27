using Assets.Modules.Graphs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class ParallelCoordinatesManager : MonoBehaviour
    {
        public GraphManager Manager;
        public ParallelCoordinatesVisualisation Template;
        private List<ParallelCoordinatesVisualisation> _connections = new List<ParallelCoordinatesVisualisation>();

        private void OnEnable()
        {
            Manager.OnGraphAdded += SyncConnectionCount;
            Manager.OnGraphDeleted += SyncConnectionCount;
            SyncConnectionCount(null);
        }

        private void OnDisable()
        {
            Manager.OnGraphAdded -= SyncConnectionCount;
            Manager.OnGraphDeleted -= SyncConnectionCount;

            while (_connections.Count > 0)
            {
                RemoveConnection(_connections.Last());
            }
        }

        private void Update()
        {
            var graphs = Manager.GetAllGraphs();
            var orderedGraphs = graphs
                .Where(g => !g.Graph.IsNewlyCreated)
                .OrderBy(g => g.Layout.Position)
                .ToArray();

            Debug.Assert(orderedGraphs.Length == _connections.Count - 1);

            for (var i = 0; i < orderedGraphs.Length - 1; i++)
            {
                var connection = _connections[i];
                connection.Left = orderedGraphs[i];
                connection.Right = orderedGraphs[i + 1];
            }
        }


        private void SyncConnectionCount(GraphMetaData graph)
        {
            var graphs = Manager.GetAllGraphs();
            var graphCount = graphs.Count(g => !g.Graph.IsNewlyCreated);

            while (graphCount < _connections.Count - 1)
            {
                RemoveConnection(_connections.Last());
            }
            
            while (graphCount > _connections.Count - 1)
            {
                CreateConnection();
            }
        }

        private ParallelCoordinatesVisualisation CreateConnection()
        {
            var connection = Instantiate(Template);
            _connections.Add(connection);
            connection.transform.parent = transform;
            return connection;
        }

        private void RemoveConnection(ParallelCoordinatesVisualisation connection)
        {
            _connections.Remove(connection);
            Destroy(connection);
        }
    }
}
