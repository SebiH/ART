using Assets.Modules.Core;
using Assets.Modules.Core.Animations;
using Assets.Modules.Tracking;
using UnityEngine;

namespace Assets.Modules.Graphs.Visualisation
{
    [RequireComponent(typeof(PointRenderer))]
    public class GraphDataField : MonoBehaviour
    {
        const float POINT_ANIMATION_LENGTH = 0.5f;

        private Graph _graph;
        private PointRenderer _pointRenderer;

        private bool _hasData = false;
        private Vec2ArrayAnimation _posAnimation = new Vec2ArrayAnimation(POINT_ANIMATION_LENGTH);

        private void OnEnable()
        {
            _pointRenderer = GetComponent<PointRenderer>();
            _graph = UnityUtility.FindParent<Graph>(this);
            _graph.OnDataChange += OnDataChange;
            OnDataChange();

            _posAnimation.Update += SetData;
        }

        private void OnDisable()
        {
            _graph.OnDataChange -= OnDataChange;
        }


        private void Update()
        {
            if ((transform.position - SceneCameraTracker.Instance.transform.position).magnitude > Globals.DataViewDistance)
            {
                _pointRenderer.SetHidden(true);
            }
            else
            {
                _pointRenderer.SetHidden(false);
            }
        }

        private void OnDataChange()
        {
            if (_graph.DimX == null || _graph.DimY == null)
            {
                _pointRenderer.SetHidden(true);
                _hasData = false;
            }
            else
            {
                var data = _graph.GetDataPosition();
                if (_hasData)
                {
                    _posAnimation.Restart(data);
                }
                else
                {
                    SetData(data);
                    _posAnimation.Init(data);
                    _hasData = true;
                }
                _pointRenderer.SetHidden(false);
            }
        }

        private void SetData(Vector2[] data)
        {
            if (_pointRenderer.Points.Length != data.Length)
            {
                _pointRenderer.Resize(data.Length);
            }

            for (var i = 0; i < data.Length; i++)
            {
                _pointRenderer.Points[i].Position = data[i];
            }

            _pointRenderer.UpdatePositions();
        }

        public void SetColors(Color32[] colors)
        {
            if (_pointRenderer.Points.Length != colors.Length)
            {
                _pointRenderer.Resize(colors.Length);
            }

            for (var i = 0; i < colors.Length; i++)
            {
                _pointRenderer.Points[i].Color = colors[i];
            }

            _pointRenderer.UpdateColor();
        }
    }
}
