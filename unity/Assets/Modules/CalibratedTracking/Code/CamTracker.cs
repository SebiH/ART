using UnityEngine;

namespace Assets.Modules.CalibratedTracking
{
    public abstract class CamTracker
    {
        public abstract void PreparePose();
        public abstract Vector3 GetPosition();
        public abstract Quaternion GetRotation();
    }
}
