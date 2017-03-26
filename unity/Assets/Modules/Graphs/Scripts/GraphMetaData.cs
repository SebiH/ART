using Assets.Modules.Graphs.Visualisation;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class GraphMetaData : MonoBehaviour
    {
        public Graph Graph;
        public GraphPosition Position;
        public GraphAnimator Animator;
        public GraphVisualisation Visualisation;

        private void OnEnable()
        {

        }
    }
}
