using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class VrTrackedObject : MonoBehaviour
    {
        private void Update()
        {
            transform.position = VRListener.CurrentPosition;
            transform.rotation = VRListener.CurrentRotation;
        }
    }
}
