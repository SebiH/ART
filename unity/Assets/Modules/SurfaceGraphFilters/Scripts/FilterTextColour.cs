using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using Assets.Modules.Graphs.Visualisation;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphFilters.Scripts
{
    [RequireComponent(typeof(GraphLabel))]
    public class FilterTextColour : MonoBehaviour
    {
        private Graph _graph;
        private GraphLabel _label;
        private ColorAnimation _colorAnimation = new ColorAnimation(Globals.NormalAnimationSpeed);

        public bool IsXAxis = true;

        private void OnEnable()
        {
            _graph = UnityUtility.FindParent<Graph>(this);
            _colorAnimation.Init(new Color32(255, 255, 255, 255));
            _label = GetComponent<GraphLabel>();
        }

        private void Update()
        {
            var isColored = IsXAxis ? _graph.UseColorX : _graph.UseColorY;
            var targetColor = isColored ? Globals.FilterActiveColor : new Color32(255, 255, 255, 255);

            if (_colorAnimation.End.r != targetColor.r || _colorAnimation.End.g != targetColor.g || _colorAnimation.End.b != targetColor.b)
            {
                _colorAnimation.Restart(targetColor);
            }

            _label.Front.material.color = _colorAnimation.CurrentValue;
            _label.Back.material.color = _colorAnimation.CurrentValue;
        }
    }
}
