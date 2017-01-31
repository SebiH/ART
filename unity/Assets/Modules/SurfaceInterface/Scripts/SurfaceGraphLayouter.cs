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

        private void OnEnable()
        {
            _surface = UnityUtility.FindParent<Surface>(this);
            _manager = GetComponent<GraphManager>();
        }

        private void Update()
        {
            foreach (var graph in _manager.GetAllGraphs())
            {
                graph.transform.localRotation = Quaternion.Euler(0, 90, 0);
                var actualPosition = graph.Position + graph.Width / 2;
                graph.transform.localPosition = new Vector3(actualPosition, 0.5f, 0.5f);
            }
        }
    }
}
