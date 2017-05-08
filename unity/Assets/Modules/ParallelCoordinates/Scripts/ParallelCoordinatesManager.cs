using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class ParallelCoordinatesManager : MonoBehaviour
    {
        public static ParallelCoordinatesManager Instance { get; private set; }

        public GraphManager Manager;
        public ParallelCoordinatesVisualisation Template;
        private List<ParallelCoordinatesVisualisation> _connections = new List<ParallelCoordinatesVisualisation>();
        private ColorsAnimation _colorAnimation = new ColorsAnimation(0.5f);

        private void OnEnable()
        {
            Instance = this;

            Manager.OnGraphAdded += SyncConnectionCount;
            Manager.OnGraphDeleted += SyncConnectionCount;
            _colorAnimation.Update += AnimateColors;
            _colorAnimation.Init(new Color32[0]);
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

            if (_connections.Count != Mathf.Max(orderedGraphs.Length - 1, 0))
            {
                SyncConnectionCount(null);
            }

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

            while (Mathf.Max(graphCount - 1, 0) < _connections.Count)
            {
                var connection = _connections.FirstOrDefault(c => c.Left == graph || c.Right == graph);
                if (!connection)
                {
                    connection = _connections.Last();
                }

                var adjustConnection = _connections.FirstOrDefault(c => c.Left == connection.Right);
                if (adjustConnection)
                {
                    adjustConnection.Left = null;
                }

                RemoveConnection(connection);
            }
            
            while (graphCount > _connections.Count + 1)
            {
                CreateConnection(graph);
            }
        }

        private ParallelCoordinatesVisualisation CreateConnection(GraphMetaData graph)
        {
            var connection = Instantiate(Template);
            connection.SetColors(_colorAnimation.CurrentValue);
            if (graph) { graph.Visualisation.DataField.SetColors(_colorAnimation.CurrentValue); }
            connection.transform.parent = transform;

            var orderedGraphs = Manager.GetAllGraphs()
                .Where(g => !g.Graph.IsNewlyCreated)
                .OrderBy(g => g.Layout.Position)
                .ToList();

            var graphPos = orderedGraphs.IndexOf(graph);
            if (graphPos < 0 || graphPos >= _connections.Count)
            {
                _connections.Add(connection);
            }
            else
            {
                _connections.Insert(graphPos, connection);
            }

            return connection;
        }

        private void RemoveConnection(ParallelCoordinatesVisualisation connection)
        {
            _connections.Remove(connection);
            Destroy(connection.gameObject);
        }

        // colorindex == data index
        public void SetColors(Color32[] colors)
        {
            if (_colorAnimation.CurrentValue.Length != colors.Length)
            {
                _colorAnimation.Init(new Color32[colors.Length]);
            }
            else
            {
                _colorAnimation.Restart(colors);
            }
        }


        private void AnimateColors(Color32[] colors)
        {
            foreach (var connection in _connections)
            {
                connection.SetColors(colors);
            }

            foreach (var graph in Manager.GetAllGraphs())
            {
                graph.Visualisation.DataField.SetColors(colors);
            }
        }
    }
}
