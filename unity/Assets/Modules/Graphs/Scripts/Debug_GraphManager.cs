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
                var g = _graphManager.CreateGraph(i);
                g.Graph.Id = i;
                g.Graph.Color = Random.ColorHSV();
                g.Graph.DimX = GetRandomData("X" + i);
                g.Graph.DimY = GetRandomData("Y" + i);
                g.Graph.gameObject.SetActive(true);
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
                var rndValues = new Dimension.DimData[NumData];
                for (var i = 0; i < rndValues.Length; i++)
                {
                    rndValues[i].Id = i;
                    rndValues[i].Value = Random.Range(-0.5f, 0.5f);
                }

                var dimension = new MetricDimension
                {
                    DisplayName = name,
                    DomainMin = -0.5f,
                    DomainMax = 0.5f,
                    Data = rndValues
                };
                dimension.PossibleTicks = new[] { -0.4f, 0, 0.4f };
                dimension.RebuildData();

                _rndValues.Add(name, dimension);
            }

            return _rndValues[name];
        }

        private void SetPositions()
        {
            foreach (var g in _graphManager.GetAllGraphs())
            {
                var pos = g.Graph.Id * SpaceBetweenGraphs;
                g.Graph.transform.localPosition = new Vector3(pos, 0, 0);
                g.Graph.transform.localRotation = Quaternion.Euler(0, 90, 0);
                g.Layout.Position = pos;
            }
        }
    }
}
