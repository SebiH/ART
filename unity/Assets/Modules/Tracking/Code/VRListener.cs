using UnityEngine;

namespace Assets.Modules.Tracking
{
    public static class VRListener
    {
        public static Vector3 CurrentPosition { get; set; }
        public static Quaternion CurrentRotation { get; set; }
        public static float PoseUpdateTime { get; set; }
    }
}
