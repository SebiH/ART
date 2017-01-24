using Assets.Modules.Calibration_Deprecated;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class TableSetup_Optitrack : MonoBehaviour
    {
        public float MarginLeft = 0.006f;
        public float MarginTop = 0.005f;
        public float OffsetY = -0.03f;

        public bool InvertUpDirection;

        public GameObject InteractiveSurfaceTemplate;

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }
    }
}
