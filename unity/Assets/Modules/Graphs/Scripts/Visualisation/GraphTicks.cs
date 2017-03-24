using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    public class GraphTicks : MonoBehaviour
    {
        public GraphLabel TickTemplate;
        private readonly List<GraphLabel> _ticks = new List<GraphLabel>();

        private Dimension _sourceDimension;
        public Dimension SourceDimension
        {
            get { return _sourceDimension; }
            set
            {
                if (_sourceDimension != value)
                {
                    _sourceDimension = value;
                    BuildTicks();
                }
            }
        }



        private void BuildTicks()
        {
            ClearTicks();

            if (_sourceDimension is MetricDimension)
            {
            }
            else if (_sourceDimension is CategoricalDimension)
            {

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
