using Assets.Modules.Core;
using Assets.Modules.Graphs;
using Assets.Modules.Surfaces;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface
{
    [RequireComponent(typeof(GraphManager))]
    public class SurfaceGraphLayouter : MonoBehaviour
    {
        public bool IsGraphSelected = false;

        private GraphManager _manager;
        private Surface _surface;

        // avoid recalculating value
        private readonly Quaternion GraphRotation = Quaternion.Euler(0, 90, 0);
        // for selection, etc.
        const float NormalAnimationSpeed = 5f;
        // for scrolling, smoothing out values from webapp
        const float FastAnimationSpeed = 15f;


        private void OnEnable()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
            _manager = GetComponent<GraphManager>();
        }

        private void Update()
        {
            foreach (var graph in _manager.GetAllGraphs())
            {
                graph.transform.localRotation = GraphRotation;

                // TODO: minor performance improvement: only calculate once globally for all graphs?
                // selection animation
                var currentOffset = graph.transform.localPosition.z;
                var targetOffset = IsGraphSelected ? 1f : 0.5f;
                var actualOffset = Mathf.Lerp(currentOffset, targetOffset, Time.unscaledDeltaTime * NormalAnimationSpeed);

                // smooth out scrolling
                var currentPosition = graph.transform.localPosition.x;
                var targetPosition = graph.Position + graph.Width / 2;
                var actualPosition = Mathf.Lerp(currentPosition, targetPosition, Time.unscaledDeltaTime * FastAnimationSpeed);

                graph.transform.localPosition = new Vector3(actualPosition, 0.5f, actualOffset);
            }
        }
    }
}
