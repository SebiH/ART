using UnityEngine;
using Assets.Modules.Tracking;

namespace Assets.Modules.Calibration_Deprecated
{
    public class Debug_PerpetualTrackNearestMarker : MonoBehaviour
    {
        public Perpetual_Calibration CalibrationScript;
        public ArucoTrackedObject TrackingScript;
        
        void Update()
        {

            if (CalibrationScript.UseMultiMarker)
            {
                if (CalibrationScript.__nearestMarkerIds.Count > 0)
                {
                    TrackingScript.TrackedId = CalibrationScript.__nearestMarkerIds[0];
                }
            }
            else
            {
                TrackingScript.TrackedId = CalibrationScript.__nearestMarkerId;
            }

            transform.localScale = Vector3.one * ArucoListener.Instance.MarkerSizeInMeter;
        }
    }
}
