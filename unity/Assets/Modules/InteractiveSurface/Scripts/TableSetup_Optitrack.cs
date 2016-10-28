using Assets.Modules.Calibration;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class TableSetup_Optitrack : MonoBehaviour
    {
        public DisplayMarker_SetupDisplay _cornerScript;

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


        private Quaternion CalculateTableRotation()
        {
            var markerBottomLeft = _cornerScript.CalibratedCorners.First((m) => m.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomLeft);
            var markerTopLeft = _cornerScript.CalibratedCorners.First((m) => m.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopLeft);
            var markerBottomRight = _cornerScript.CalibratedCorners.First((m) => m.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomRight);

            var forward = Vector3.Normalize(markerTopLeft.Position - markerBottomLeft.Position);
            var right = Vector3.Normalize(markerBottomRight.Position - markerBottomLeft.Position);
            var up = Vector3.Cross(forward, right);
            // Cross product doesn't always point in the correct direction
            if (InvertUpDirection) { up = -up; }

            return Quaternion.LookRotation(forward, up);
        }

        private bool lrs = false;
        void Update()
        {
            if (_cornerScript.CalibratedCorners.Count >= 4 && !lrs)
            {
                lrs = true;
                var lineRenderer = GetComponent<LineRenderer>();
                var tableRotation = CalculateTableRotation();

                lineRenderer.SetVertexCount(5);
                var topleft = _cornerScript.CalibratedCorners.First((c) => c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopLeft).Position;
                topleft = topleft + tableRotation * new Vector3(-MarginLeft, OffsetY, MarginTop);
                var bottomleft = _cornerScript.CalibratedCorners.First((c) => c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomLeft).Position;
                bottomleft = bottomleft + tableRotation * new Vector3(MarginLeft, OffsetY, MarginTop);
                var bottomright = _cornerScript.CalibratedCorners.First((c) => c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomRight).Position;
                bottomright = bottomright + tableRotation * new Vector3(MarginLeft, OffsetY, -MarginLeft);
                var topright = _cornerScript.CalibratedCorners.First((c) => c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopRight).Position;
                topright = topright + tableRotation * new Vector3(MarginLeft, OffsetY, MarginTop);

                lineRenderer.SetPosition(0, topleft);
                lineRenderer.SetPosition(1, bottomleft);
                lineRenderer.SetPosition(2, bottomright);
                lineRenderer.SetPosition(3, topright);
                lineRenderer.SetPosition(4, topleft);
            }
        }

    }
}
