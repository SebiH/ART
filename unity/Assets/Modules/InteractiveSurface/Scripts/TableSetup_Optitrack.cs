using Assets.Modules.Calibration;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class TableSetup_Optitrack : MonoBehaviour
    {
        public DisplayMarker_SetupDisplay _cornerScript;

        public float MarginLeft = 0.047f;
        public float MarginTop = 0.046f;
        public float OffsetY = -0.03f;

        public GameObject InteractiveSurfaceTemplate;

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }


        private void Update()
        {
            if (_cornerScript.CalibratedCorners.Count >= 4)
            {
                var lineRenderer = GetComponent<LineRenderer>();

                lineRenderer.SetVertexCount(5);
                var start = _cornerScript.CalibratedCorners.First((c) => c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopLeft);
                lineRenderer.SetPosition(0, start.Position);


                foreach (var corner in _cornerScript.CalibratedCorners)
                {
                    var nextCorner = _cornerScript.CalibratedCorners.First((c) =>
                    {
                        switch (corner.Corner)
                        {
                            case DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopLeft:
                                return c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomLeft;
                            case DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomLeft:
                                return c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomRight;
                            case DisplayMarker_PerformCalibration.MarkerOffset.Corner.BottomRight:
                                return c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopRight;
                            case DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopRight:
                                return c.Corner == DisplayMarker_PerformCalibration.MarkerOffset.Corner.TopLeft;
                        }

                        return false;
                    });
                }
            }
        }

    }
}
