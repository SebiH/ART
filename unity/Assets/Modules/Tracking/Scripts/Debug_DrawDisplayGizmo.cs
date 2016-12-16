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

                Gizmos.color = Color.white;

                var tl = display.GetCornerPosition(Corner.TopLeft);
                var tr = display.GetCornerPosition(Corner.TopRight);
                var bl = display.GetCornerPosition(Corner.BottomLeft);
                var br = display.GetCornerPosition(Corner.BottomRight);

                Gizmos.DrawLine(tl, bl);
                Gizmos.DrawLine(bl, br);
                Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl);
            }
        }
    }
}
