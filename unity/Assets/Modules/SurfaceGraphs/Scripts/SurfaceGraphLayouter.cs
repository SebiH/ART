using Assets.Modules.Graphs;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    [RequireComponent(typeof(GraphManager))]
    public class SurfaceGraphLayouter : MonoBehaviour
    {
        private GraphManager _manager;

        private void OnEnable()
        {
            _manager = GetComponent<GraphManager>();
        }

        private void Update()
        {
            var isGraphSelected = _manager.GetAllGraphs().Any(g => g.IsSelected);

            foreach (var graph in _manager.GetAllGraphs())
            {
                var animator = graph.GetComponent<GraphAnimator>();

                // TODO: minor performance improvement: only calculate once globally for all graphs?
                // selection animation
                var targetOffset = isGraphSelected ? 1f : 0.7f;
                if (graph.IsSelected) { targetOffset = 0.5f; }
                animator.Offset = targetOffset;
                animator.Position = graph.Position;
                animator.Height = 0.5f;

                var rotY = graph.IsSelected ? 0 : 90;
                var rotZ = graph.IsFlipped ? 90 : 0;
                var targetRotation = graph.IsSelected ? Quaternion.Euler(0, rotY, rotZ) : Quaternion.Euler(0, rotY, rotZ);
                animator.Rotation = targetRotation;
            }
        }
    }
}
