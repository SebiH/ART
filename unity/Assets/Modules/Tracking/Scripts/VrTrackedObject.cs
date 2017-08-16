using UnityEngine;
using UnityEngine.VR;

namespace Assets.Modules.Tracking
{
    public class VrTrackedObject : MonoBehaviour
    {
        public bool UseVrListener = true;
        public VRNode node;

        private void Update()
        {
            if (UseVrListener)
            {
                transform.position = VRListener.CurrentPosition;
                transform.rotation = VRListener.CurrentRotation;
            }
            else
            {
                transform.rotation = InputTracking.GetLocalRotation(node);
                transform.position = InputTracking.GetLocalPosition(node);
            }
        }
    }
}
