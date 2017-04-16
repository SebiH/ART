using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Core
{
    public class CopyRenderQueue : MonoBehaviour
    {
        public Renderer Source;
        public int Offset = 0;
        private Material _target;

        private void Start()
        {
            if (GetComponent<CanvasRenderer>())
            {
                _target = GetComponent<CanvasRenderer>().GetMaterial();
            }
            else if (GetComponent<Renderer>())
            {
                var renderer = GetComponent<Renderer>();
                if (renderer.materials.Length == 1)
                {
                    _target = renderer.material;
                }
                else
                {
                    _target = renderer.materials[renderer.materials.Length - 1];
                }
            }

            if (_target == null && GetComponent<Image>())
            {
                _target = GetComponent<Image>().material;
            }
            if (_target == null && GetComponent<Text>())
            {
                _target = GetComponent<Text>().material;
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
            else
            {
                Debug.LogWarning("Cannot copy renderqueue - no source found");
            }

            if (_target != null)
            {
                _target.renderQueue = renderQueue + Offset;
            }
            else
            {
                Debug.LogWarning("Cannot copy renderqueue - no target! " + gameObject.name);
            }
        }
    }
}
