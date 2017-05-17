using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using Assets.Modules.Surfaces;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    [RequireComponent(typeof(GraphManager))]
    public class GraphManagerLayout : MonoBehaviour
    {
        const float OFFSET_SELECTED = 1.25f;

        private Surface _surface;
        private GraphManager _graphManager;
        private ValueAnimation _offsetAnimation = new ValueAnimation(Globals.NormalAnimationSpeed);
        private bool _wasGraphSelected = false;

        private void OnEnable()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
            _graphManager = GetComponent<GraphManager>();
            _offsetAnimation.Init(0);
        }

        private void Update()
        {
            var isAnyGraphSelected = _graphManager.GetAllGraphs().Any(g => g.Graph.IsSelected);

            if (isAnyGraphSelected)
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
                    _offsetAnimation.Restart(_surface.Offset);
                }
            }

            if (_offsetAnimation.IsRunning)
            {
                transform.localPosition = new Vector3(0, 0, _offsetAnimation.CurrentValue);
            }
            else if (isAnyGraphSelected)
            {
                transform.localPosition = new Vector3(0, 0, OFFSET_SELECTED);
            }
            else
            {
                transform.localPosition = new Vector3(0, 0, _surface.Offset);
                _offsetAnimation.Init(_surface.Offset);
            }
        }
    }
}
