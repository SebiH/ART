using UnityEngine;
using UnityEngine.VR;

namespace Assets.Modules.Tracking
{
    public class UnityVrTracker : MonoBehaviour
    {
        public VRNode Node = VRNode.CenterEye;

        private void FixedUpdate()
        {
            VRListener.CurrentPosition = InputTracking.GetLocalPosition(Node);
            VRListener.CurrentRotation = InputTracking.GetLocalRotation(Node);
            VRListener.PoseUpdateTime = Time.unscaledTime;
        }
    }
}
