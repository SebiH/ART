using System.Linq;
using UnityEngine;

namespace Assets.Modules.Core
{
    public class TransparencyRenderQueueSorter : MonoBehaviour
    {
        public Transform TrackedCamera;

        private void Update()
        {
            var transparentObjs = GameObject.FindGameObjectsWithTag("Transparent");
            var sortedObjs = transparentObjs.OrderBy(t => { return Mathf.Abs(Vector3.Distance(t.transform.position, TrackedCamera.transform.position)); });
            var queueCounter = 4000;

            foreach (var obj in sortedObjs)
            {
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer)
                {
                    // minor hack for multi materials: transparent material is always last
                    if (renderer.materials.Length >= 2)
                    {
                        renderer.materials[renderer.materials.Length - 1].renderQueue = queueCounter--;
                    }
                    else
                    {
                        renderer.material.renderQueue = queueCounter--;
                    }
                }
                else
                {
                    var skinnedRenderer = obj.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedRenderer)
                    {
                        // minor hack for multi materials: transparent material is always last
                        if (skinnedRenderer.materials.Length >= 2)
                        {
                            skinnedRenderer.materials[skinnedRenderer.materials.Length - 1].renderQueue = queueCounter--;
                        }
                        else
                        {
                            skinnedRenderer.material.renderQueue = queueCounter--;
                        }
                    }
                }
            }
        }
    }
}
