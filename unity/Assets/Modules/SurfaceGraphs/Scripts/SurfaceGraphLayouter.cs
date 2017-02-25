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
        const float NormalAnimationSpeed = 0.7f;
        // for scrolling, smoothing out values from webapp
        const float FastAnimationSpeed = 0.05f;


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
                var targetOffset = _isGraphSelected ? 1f : 0.5f;
                if (graph.IsSelected) { targetOffset = 0.5f; }
                var actualOffset = Mathf.Lerp(currentOffset, targetOffset, Time.unscaledDeltaTime / NormalAnimationSpeed);
                actualOffset = Roughly(actualOffset, targetOffset);

                // smooth out scrolling
                var currentPosition = graph.transform.localPosition.x;
                var targetPosition = graph.Position;
                var positionAnimationSpeed = graph.IsSelected ? NormalAnimationSpeed : FastAnimationSpeed;
                var actualPosition = Mathf.Lerp(currentPosition, targetPosition, Time.unscaledDeltaTime / positionAnimationSpeed);
                actualPosition = Roughly(actualPosition, targetPosition);

                // creation / deletion
                var currentHeight = graph.transform.localPosition.y;
                var targetHeight = 0.5f;
                var actualHeight = Mathf.Lerp(currentHeight, targetHeight, Time.unscaledDeltaTime / NormalAnimationSpeed);
                actualHeight = Roughly(actualHeight, targetHeight);

                var currentRotation = graph.transform.localRotation;
                var targetRotation = graph.IsSelected ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);
                var actualRotation = Quaternion.Lerp(currentRotation, targetRotation, Time.unscaledDeltaTime / NormalAnimationSpeed);
                actualRotation = Roughly(actualRotation, targetRotation);

                graph.transform.localPosition = new Vector3(actualPosition, actualHeight, actualOffset);
                graph.transform.localScale = new Vector3(graph.Scale, graph.Scale, 1);
                graph.transform.localRotation = actualRotation;


                graph.IsAnimating = (Mathf.Abs(targetHeight - actualHeight) > Mathf.Epsilon || Mathf.Abs(Quaternion.Angle(currentRotation, actualRotation)) > Mathf.Epsilon);
            }
        }


        private void HandleNewGraph(Graph graph)
        {
            graph.transform.localRotation = Quaternion.Euler(0, 90, 0);
            graph.transform.localPosition = new Vector3(0, -0.5f, 0.5f);
            graph.Scale = 0.55f;
        }

        private float Roughly(float current, float target)
        {
            if (Mathf.Abs(target - current) < 0.001f)
                return target;
            return current;
        }

        private Quaternion Roughly(Quaternion current, Quaternion target)
        {
            if (Mathf.Abs(Quaternion.Angle(current, target)) < 1f)
                return target;
            return current;
        }
    }
}
