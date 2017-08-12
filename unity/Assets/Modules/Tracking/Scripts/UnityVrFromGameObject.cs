using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class UnityVrFromGameObject : MonoBehaviour
    {

        private void FixedUpdate()
        {
            VRListener.CurrentPosition = transform.position;
            VRListener.CurrentRotation = transform.rotation;
            VRListener.PoseUpdateTime = Time.unscaledTime;
        }
    }
}
