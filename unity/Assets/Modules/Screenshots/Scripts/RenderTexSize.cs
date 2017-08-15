using UnityEngine;

namespace Assets.Modules.Screenshots
{
    public class RenderTexSize : MonoBehaviour
    {
        public Camera SourceCamera;
        public float FocalPoint = 0.0215f;

        private void Update()
        {
            // https://forum.unity3d.com/threads/how-to-calculate-horizontal-field-of-view.16114/
            var radAngle = SourceCamera.fieldOfView * Mathf.Deg2Rad;
            var radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * SourceCamera.aspect);
            var hFOV = Mathf.Rad2Deg * radHFOV;

            var width = FocalPoint * Mathf.Tan(Mathf.Deg2Rad * hFOV / 2f) * 2f;
            var height = FocalPoint * Mathf.Tan(Mathf.Deg2Rad * SourceCamera.fieldOfView / 2f) * 2f;

            transform.localScale = new Vector3(width, height, 1.0f);
            transform.localPosition = new Vector3(0, 0, FocalPoint);
        }

    }
}
