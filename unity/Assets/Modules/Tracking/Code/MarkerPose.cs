using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class MarkerPose
    {
        public int Id;
        public string Name = "";
        public Vector3 Position;
        public Quaternion Rotation;
        public readonly float DetectionTime;

        public MarkerPose()
        {
            DetectionTime = Time.unscaledTime;
        }
    }
}
