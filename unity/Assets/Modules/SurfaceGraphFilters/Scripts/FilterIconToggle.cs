using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using Assets.Modules.Graphs;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.SurfaceGraphFilters
{
    public class FilterIconToggle : MonoBehaviour
    {
        private GraphFilterListener _filterListener;
        private Graph _graph;
        private bool _isEnabled = false;

        private QuaternionAnimation _rotationAnimation = new QuaternionAnimation(Globals.NormalAnimationSpeed);
        private ColorAnimation _colorAnimation = new ColorAnimation(Globals.NormalAnimationSpeed);

        private void Start()
        {
            _graph = UnityUtility.FindParent<Graph>(this);

            _filterListener = UnityUtility.FindParent<GraphFilterListener>(this);
            if (_filterListener)
            {
                _filterListener.OnFilterUpdate += OnFilterUpdate;
                OnFilterUpdate();
            }

            _rotationAnimation.Init(Quaternion.identity);
            _colorAnimation.Init(new Color32(255, 255, 255, 0));
            StartCoroutine(RunUpdates());
        }

        private void OnDestroy()
        {
            if (_filterListener)
            {
                _filterListener.OnFilterUpdate -= OnFilterUpdate;
            }
        }

        private IEnumerator RunUpdates()
        {
            yield return new WaitForEndOfFrame();

            while (isActiveAndEnabled)
            {
                var children = GetComponentsInChildren<Image>();
                ApplyChanges(children);
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void ApplyChanges(Image[] children)
        {
            var targetRotation = _graph.IsFlipped ? Quaternion.Euler(0, 0, -90) : Quaternion.identity;
            if (_rotationAnimation.End != targetRotation)
            {
                _rotationAnimation.Restart(targetRotation);
            }

            transform.localRotation = _rotationAnimation.CurrentValue;

            var targetColor = _graph.IsColored ? Globals.FilterActiveColor : new Color32(255, 255, 255, 255);
            targetColor.a = (byte)(_isEnabled ? 255 : 0);

            if (_colorAnimation.End.r != targetColor.r || _colorAnimation.End.g != targetColor.g || _colorAnimation.End.b != targetColor.b || _colorAnimation.End.a != targetColor.a)
            {
                _colorAnimation.Restart(targetColor);
            }

            foreach (var icon in children)
            {
                icon.material.color = _colorAnimation.CurrentValue;
                icon.enabled = _colorAnimation.CurrentValue.a > 0;
            }
        }

        private void OnFilterUpdate()
        {
            var filters = _filterListener.GetFilters();
            _isEnabled = filters.Any(f => f.origin == _graph.Id);
        }
    }
}
