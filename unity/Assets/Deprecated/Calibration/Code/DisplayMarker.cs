using Assets.Modules.Surfaces;
using System;
using UnityEngine;

namespace Assets.Modules.Calibration_Deprecated
{
    [Serializable]
    public class DisplayMarker
    {
        public int id;
        public int posX;
        public int posY;
        public int size;

        public Vector3 GetUnityPosition(Surface surface)
        {
            /*
             * Display:
             *       x
             *   +------->
             *   |
             *  y|
             *   v
             *
             * Unity:
             *  ^   /
             * y|  /z
             *  | /
             *  +--------
             *    x
             *
             *
             * (x,y)_display
             * <=>
             * (x,-z)_unity (display assumed to be horizontal in unity coords)
             */

            // posX/Y points to topleft corner of marker; we need center for calibration purposes
            var markerOffset = surface.PixelToUnityCoord(size) / 2f;

            // origin of marker coordinates is top-left corner;
            var unityPosX = surface.PixelToUnityCoord(posX) + markerOffset;
            var unityPosY = 0f; // marker lies directly on display
            var unityPosZ = -(surface.PixelToUnityCoord(posY) + markerOffset);

            return new Vector3(unityPosX, unityPosY, unityPosZ);
        }
    }
}
