using Assets.Modules.Graphs;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    [RequireComponent(typeof(GraphManager))]
    public class SurfaceGraphLayouter : MonoBehaviour
    {
        private GraphManager _manager;
        private bool _isGraphSelected = false;

        // for selection, etc.
        const float NormalAnimationSpeed = 1f;
        // for scrolling, smoothing out values from webapp
        const float FastAnimationSpeed = 20f;


        private void OnEnable()
        {
            _manager = GetComponent<GraphManager>();
            _manager.OnGraphAdded += HandleNewGraph;
        }

        private void OnDisable()
        {
            _manager.OnGraphAdded -= HandleNewGraph;
        }

        private void Update()
        {
            _isGraphSelected = _manager.GetAllGraphs().Any(g => g.IsSelected);

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
                graph.transform.localScale = new Vector3(graph.Scale, graph.Scale, 1);
            }
        }


        private void HandleNewGraph(Graph graph)
        {
            graph.transform.localRotation = Quaternion.Euler(0, 90, 0);
            graph.transform.localPosition = new Vector3(0, -0.5f, GetZOffset());
            graph.Scale = 0.55f;
        }

        private float GetZOffset()
        {
            return _isGraphSelected ? 1f : 0.5f;
        }
    }
}
