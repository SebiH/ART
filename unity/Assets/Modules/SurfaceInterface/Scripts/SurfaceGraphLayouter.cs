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

        // for selection, etc.
        const float NormalAnimationSpeed = 1f;
        // for scrolling, smoothing out values from webapp
        const float FastAnimationSpeed = 20f;


        private void OnEnable()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
            _manager = GetComponent<GraphManager>();
            _manager.OnGraphAdded += HandleNewGraph;
        }

        private void OnDisable()
        {
            _manager.OnGraphAdded -= HandleNewGraph;
        }

        private void Update()
        {
            foreach (var graph in _manager.GetAllGraphs())
            {
                // TODO: minor performance improvement: only calculate once globally for all graphs?
                // selection animation
                var currentOffset = graph.transform.localPosition.z;
                var targetOffset = GetZOffset();
                var actualOffset = Mathf.Lerp(currentOffset, targetOffset, Time.unscaledDeltaTime * NormalAnimationSpeed);

                // smooth out scrolling
                var currentPosition = graph.transform.localPosition.x;
                var targetPosition = graph.Position;
                var actualPosition = Mathf.Lerp(currentPosition, targetPosition, Time.unscaledDeltaTime * FastAnimationSpeed);

                // creation / deletion
                var currentHeight = graph.transform.localPosition.y;
                var targetHeight = 0.5f;
                var actualHeight = Mathf.Lerp(currentHeight, targetHeight, Time.unscaledDeltaTime * NormalAnimationSpeed);

                graph.transform.localPosition = new Vector3(actualPosition, actualHeight, actualOffset);
            }
        }


        private void HandleNewGraph(Graph graph)
        {
            graph.transform.localRotation = Quaternion.Euler(0, 90, 0);
            graph.transform.localPosition = new Vector3(0, -0.5f, GetZOffset());
            var scale = 0.75f;
            graph.transform.localScale = new Vector3(scale, scale, 1);
        }

        private float GetZOffset()
        {
            return IsGraphSelected ? 1f : 0.5f;
        }
    }
}
