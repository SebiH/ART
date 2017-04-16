using Assets.Modules.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    public class GraphTicks : MonoBehaviour
    {
        public GraphLabel TickTemplateX;
        public GraphLabel TickTemplateY;

        public Renderer RenderQueueSource;
        public Material MaterialTemplate;
        private Material _sharedMaterial;

        public bool IsXAxis = true;

        private readonly List<GraphLabel> _ticks = new List<GraphLabel>();
        private Graph _graph;

        private Dimension _dimension;
        public Dimension SourceDimension
        {
            get { return _dimension; }
            set
            {
                if (_dimension != value)
                {
                    _dimension = value;
                    BuildTicks();
                }
            }
        }

        private bool _wasGraphFlipped;

        private void OnEnable()
        {
            _graph = UnityUtility.FindParent<GraphMetaData>(this).Graph;
            _wasGraphFlipped = _graph.IsFlipped;
            _sharedMaterial = Instantiate(MaterialTemplate);
        }

        private void Update()
        {
            if (_graph.IsFlipped != _wasGraphFlipped)
            {
                _wasGraphFlipped = _graph.IsFlipped;
                BuildTicks();
            }

            _sharedMaterial.renderQueue = RenderQueueSource.material.renderQueue;
        }

        private void BuildTicks()
        {
            ClearTicks();

            if (_dimension != null)
            {
                foreach (var tick in _dimension.Ticks)
                {
                    var tickGO = SpawnTick(tick.Name, _dimension.Scale(tick.Value));
                    _ticks.Add(tickGO);
                }
            }
        }

        private void ClearTicks()
        {
            foreach (var tick in _ticks)
            {
                Destroy(tick.gameObject);
            }

            _ticks.Clear();
        }

        private GraphLabel SpawnTick(string name, float position)
        {
            var template = (_graph.IsFlipped ^ IsXAxis) ? TickTemplateX : TickTemplateY;
            var tick = Instantiate(template);
            tick.Text = name;

            var tickTransform = tick.transform as RectTransform;
            tickTransform.SetParent(transform, false);

            tick.Front.material = _sharedMaterial;
            tick.Back.material = _sharedMaterial;

            if (IsXAxis)
            {
                tickTransform.localPosition = new Vector3(position * 200, 0, 0);
            }
            else
            {
                tickTransform.localPosition = new Vector3(-position * 200, 0, 0);
            }

            if (IsXAxis && _graph.IsFlipped)
            {
                tickTransform.localRotation *= Quaternion.Euler(0, 180, 0);
            }

            return tick;
        }
    }
}
