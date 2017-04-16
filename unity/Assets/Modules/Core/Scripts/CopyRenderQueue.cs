using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Core
{
    public class CopyRenderQueue : MonoBehaviour
    {
        public Renderer Source;
        public int Offset = 0;
        private List<Material> _targets = new List<Material>();

        private void Start()
        {
            if (GetComponent<CanvasRenderer>())
            {
                var canvasRenderer = GetComponent<CanvasRenderer>();
                var mat = canvasRenderer.GetMaterial();
                if (mat)
                {
                    // material is shared between gui instances...
                    mat = Instantiate(mat);
                    canvasRenderer.SetMaterial(mat, 0);
                    _targets.Add(mat);
                }
            }
            else if (GetComponent<Renderer>())
            {
                var renderer = GetComponent<Renderer>();
                if (renderer.materials.Length == 1)
                {
                    _targets.Add(renderer.material);
                }
                else
                {
                    _targets.Add(renderer.materials[renderer.materials.Length - 1]);
                }
            }

            var image = GetComponent<Image>();
            if (image && image.material != null)
            {
                // material is shared between gui instances...
                var mat = Instantiate(image.material);
                image.material = mat;
                _targets.Add(mat);
            }

            var text = GetComponent<Text>();
            if (text && text.material != null)
            {
                // material is shared between gui instances...
                var mat = Instantiate(text.material);
                text.material = mat;
                _targets.Add(mat);
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

            if (_targets.Count > 0)
            {
                foreach (var target in _targets)
                {
                    target.renderQueue = renderQueue + Offset;
                }
            }
            else
            {
                Debug.LogWarning("Cannot copy renderqueue - no target! " + gameObject.name);
            }
        }
    }
}
