using Assets.Modules.InteractiveSurface;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class Debug_DrawDisplayGizmo : MonoBehaviour
    {
        public string DisplayName = "Surface";

        void OnDrawGizmos()
        {
            if (FixedDisplays.Has(DisplayName))
            {
                var display = FixedDisplays.Get(DisplayName);

                // draw coordinates as given via calibration
                Gizmos.color = Color.white;

                var tl = display.GetCornerPosition(Corner.TopLeft);
                var tr = display.GetCornerPosition(Corner.TopRight);
                var bl = display.GetCornerPosition(Corner.BottomLeft);
                var br = display.GetCornerPosition(Corner.BottomRight);

                Gizmos.DrawLine(tl, bl);
                Gizmos.DrawLine(bl, br);
                Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl);

                // draw coordinates as calculated via pixelToCm Ratio and displaySize
                Gizmos.color = Color.red;
                var uh = DisplayUtility.PixelToUnityCoord(display.DisplayResolution.height);
                var uw = DisplayUtility.PixelToUnityCoord(display.DisplayResolution.width);

                var downV = (bl - tl).normalized;
                var rightV = (tr - tl).normalized;

                Gizmos.DrawLine(tl, tl + downV * uh);
                Gizmos.DrawLine(tl + downV * uh, tl + downV * uh + rightV * uw);
                Gizmos.DrawLine(tl + downV * uh + rightV * uw, tl + rightV * uw);
                Gizmos.DrawLine(tl, tl + rightV * uw);
            }
        }
    }
}
