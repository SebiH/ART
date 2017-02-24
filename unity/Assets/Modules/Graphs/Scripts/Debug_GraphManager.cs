using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    [RequireComponent(typeof(GraphManager))]
    public class Debug_GraphManager : MonoBehaviour
    {
        public GameObject GraphTemplate;
        public int NumGraphs = 5;
        public float SpaceBetweenGraphs = 0.1f;

        private Dictionary<string, Dimension> _rndValues = new Dictionary<string, Dimension>();
        public int NumData = 100;

        private GraphManager _graphManager;

        private void OnEnable()
        {
            _graphManager = GetComponent<GraphManager>();

            for (var i = 0; i < NumGraphs; i++)
            {
                var graph = _graphManager.CreateGraph(i);
                graph.DimX = GetRandomData("X" + i);
                graph.DimY = GetRandomData("Y" + i);
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

        public Dimension GetRandomData(string name)
        {
            if (!_rndValues.ContainsKey(name))
            {
                var rndValues = new float[NumData];
                for (var i = 0; i < rndValues.Length; i++)
                {
                    rndValues[i] = Random.Range(-0.5f, 0.5f);
                }

                var dimension = new MetricDimension
                {
                    DisplayName = name,
                    DomainMin = -0.5f,
                    DomainMax = 0.5f,
                    Data = rndValues
                };

                _rndValues.Add(name, dimension);
            }

            return _rndValues[name];
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
