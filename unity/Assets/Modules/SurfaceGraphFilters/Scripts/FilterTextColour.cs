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
        private Material _material;
        private ColorAnimation _colorAnimation = new ColorAnimation(Globals.NormalAnimationSpeed);

        public bool IsXAxis = true;

        private void OnEnable()
        {
            _graph = UnityUtility.FindParent<Graph>(this);
            _colorAnimation.Init(new Color32(255, 255, 255, 255));

            // Duplicate material because material is somehow shared between UI elements??
            var label = GetComponent<GraphLabel>();
            _material = Instantiate(label.Front.material);
            label.Front.material = _material;
            label.Back.material = _material;
        }

        private void Update()
        {
            var isColored = IsXAxis ? _graph.UseColorX : _graph.UseColorY;
            var targetColor = isColored ? Globals.FilterActiveColor : new Color32(255, 255, 255, 255);

            if (_colorAnimation.End.r != targetColor.r || _colorAnimation.End.g != targetColor.g || _colorAnimation.End.b != targetColor.b)
            {
                _colorAnimation.Restart(targetColor);
            }

            _material.color = _colorAnimation.CurrentValue;
        }
    }
}
