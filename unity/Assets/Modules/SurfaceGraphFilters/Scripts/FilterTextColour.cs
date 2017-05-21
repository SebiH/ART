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
            _label = GetComponent<GraphLabel>();
            _colorAnimation.Init(new Color32(255, 255, 255, 255));
            _colorAnimation.Finished += ApplyColor;
        }

        private void OnDisable()
        {
            _colorAnimation.Finished -= ApplyColor;
        }

        private void Update()
        {
            var targetColor = _graph.IsColored ? Globals.FilterActiveColor : new Color32(255, 255, 255, 255);

            if (_colorAnimation.End.r != targetColor.r || _colorAnimation.End.g != targetColor.g || _colorAnimation.End.b != targetColor.b)
            {
                _colorAnimation.Restart(targetColor);
            }

            if (_colorAnimation.IsRunning)
            {
                ApplyColor();
            }
        }

        private void ApplyColor()
        {
            _label.Front.material.color = _colorAnimation.CurrentValue;
            _label.Back.material.color = _colorAnimation.CurrentValue;
        }
    }
}
