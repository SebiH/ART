using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.SurfaceGraphFilters
{
    [RequireComponent(typeof(Image))]
    public class FilterIconToggle : MonoBehaviour
    {
        private GraphFilterListener _filterListener;
        private Graph _graph;
        private Image _filterIcon;

        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(Globals.NormalAnimationSpeed);
        private ColorAnimation _colorAnimation = new ColorAnimation(Globals.NormalAnimationSpeed);

        private void Start()
        {
            _graph = UnityUtility.FindParent<Graph>(this);
            _filterListener = UnityUtility.FindParent<GraphFilterListener>(this);
            _filterListener.OnFilterUpdate += OnFilterUpdate;
            _filterIcon = GetComponent<Image>();

            OnFilterUpdate();

            _rotationAnimation.Init(Quaternion.identity);
            _colorAnimation.Init(new Color32(255, 255, 255, 255));
        }

        private void OnDestroy()
        {
            _filterListener.OnFilterUpdate -= OnFilterUpdate;
        }

        private void Update()
        {
            var targetRotation = _graph.IsFlipped ? Quaternion.Euler(0, 0, -90) : Quaternion.identity;
            if (_rotationAnimation.End != targetRotation)
            {
                _rotationAnimation.Restart(targetRotation);
            }

            transform.localRotation = _rotationAnimation.CurrentValue;



            var targetColor = _graph.IsColored ? new Color32(139, 195, 74, 255) : new Color32(255, 255, 255, 255);

            if (_colorAnimation.End.r != targetColor.r || _colorAnimation.End.g != targetColor.g || _colorAnimation.End.b != targetColor.b)
            {
                _colorAnimation.Restart(targetColor);
            }

            _filterIcon.color = _colorAnimation.CurrentValue;
        }

        private void OnFilterUpdate()
        {
            var filters = _filterListener.GetFilters();
            _filterIcon.enabled = filters.Any(f => f.origin == _graph.Id);
        }
    }
}
