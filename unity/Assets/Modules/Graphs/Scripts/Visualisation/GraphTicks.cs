using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    public class GraphTicks : MonoBehaviour
    {
        public GraphLabel TickTemplate;
        private readonly List<GraphLabel> _ticks = new List<GraphLabel>();

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
            var tick = Instantiate(TickTemplate);
            tick.Text = name;

            var tickTransform = tick.transform as RectTransform;
            tickTransform.SetParent(transform);
            tickTransform.localPosition = new Vector3(position * 20, 0, 0) * 2;
            tickTransform.localRotation = Quaternion.Euler(0, 0, -65);
            tickTransform.localScale = Vector3.one;

            return tick;
        }
    }
}
