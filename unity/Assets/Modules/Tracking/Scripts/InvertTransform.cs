using Assets.Modules.Core.Util;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class InvertTransform : MonoBehaviour
    {
        public Transform OriginalTransform;

        void Update()
        {
            var matrix = Matrix4x4.TRS(OriginalTransform.localPosition, OriginalTransform.localRotation, OriginalTransform.localScale);

            matrix = matrix.inverse;

            transform.localPosition = matrix.GetPosition();
            transform.localRotation = matrix.GetRotation();
            transform.localScale = matrix.GetScale();
        }
    }
}
