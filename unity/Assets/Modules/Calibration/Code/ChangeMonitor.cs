using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ChangeMonitor
    {
        // 0: unstable; 1: stable
        public float StabilityLevel { get; private set; }


        public ChangeMonitor(Vector3 initialValue)
        {

        }

        public ChangeMonitor(Quaternion intialValue)
        {

        }


        public void Update(Vector3 nextValue)
        {

        }

        public void Update(Quaternion nextValue)
        {

        }
    }
}
