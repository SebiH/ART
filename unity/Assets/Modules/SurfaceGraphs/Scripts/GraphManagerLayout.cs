using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    [RequireComponent(typeof(GraphManager))]
    public class GraphManagerLayout : MonoBehaviour
    {
        const float OFFSET_SELECTED = 0.4f;
        const float OFFSET_NORMAL = 0f;

        private GraphManager _graphManager;
        private ValueAnimation _offsetAnimation = new ValueAnimation(0.6f);
        private bool _wasGraphSelected = false;

        private void OnEnable()
        {
            _graphManager = GetComponent<GraphManager>();
            _offsetAnimation.Init(0);
        }

        private void Update()
        {
            if (_graphManager.GetAllGraphs().Any(g => g.Graph.IsSelected))
            {
                if (!_wasGraphSelected)
                {
                    _wasGraphSelected = true;
                    _offsetAnimation.Restart(OFFSET_SELECTED);
                }
            }
            else
            {
                if (_wasGraphSelected)
                {
                    _wasGraphSelected = false;
                    _offsetAnimation.Restart(OFFSET_NORMAL);
                }
            }

            transform.localPosition = new Vector3(0, 0, _offsetAnimation.CurrentValue);
        }
    }
}
