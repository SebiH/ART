using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Core
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CanvasCopyRenderQueue : MonoBehaviour
    {
        public Renderer Source;
        public int Offset = 0;
        private CanvasRenderer _alternateSource;
        private CanvasRenderer _target;

        private void OnEnable()
        {
            _target = GetComponent<CanvasRenderer>();

            if (!Source)
            {
                _alternateSource = UnityUtility.FindParent<CanvasCopyRenderQueue>(this).GetComponent<CanvasRenderer>();
            }
        }

        private void Update()
        {
            int renderQueue = 3500;
            if (Source)
            {
                if (Source.materials.Length > 1)
                {
                    renderQueue = Source.materials[Source.materials.Length - 1].renderQueue;
                }
                else
                {
                    renderQueue = Source.material.renderQueue;
                }
            }
            else if (_alternateSource)
            {
                renderQueue = _alternateSource.GetMaterial().renderQueue;
            }
            else
            {
                Debug.LogWarning("Cannot copy renderqueue - no source found");
                renderQueue = _target.GetMaterial().renderQueue;
            }

            var adjQueue = renderQueue + Offset;

            if (_target && _target.GetMaterial() != null)
            {
                _target.GetMaterial().renderQueue = adjQueue;
            }
            else
            {
                if (GetComponent<Image>() != null)
                {
                    GetComponent<Image>().material.renderQueue = adjQueue;
                }
                else if (GetComponent<Text>() != null)
                {
                    GetComponent<Text>().material.renderQueue = adjQueue;
                }
                else
                {
                    Debug.LogWarning("Cannot copy renderqueue - no target!");
                }
            }
        }
    }
}
