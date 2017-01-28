using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ChangeMonitor
    {
        /// <summary>
        /// 0: unstable - 1: stable
        /// </summary>
        public float StabilityLevel { get; private set; }


        public ChangeMonitor()
        {
            StabilityLevel = 1;
        }

        public void Update(Vector3 nextPosition, Quaternion nextRotation)
        {

        }

        public void Reset()
        {

        }
    }
}
