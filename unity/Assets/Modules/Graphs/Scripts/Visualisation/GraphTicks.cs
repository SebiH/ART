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
                // TODO
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
    }
}
