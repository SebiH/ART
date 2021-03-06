using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using System;
using UnityEngine;

namespace Assets.Modules.Calibration_Deprecated
{
    [Serializable]
    public class DisplayMap
    {
        public int id;
        public int posX;
        public int posY;
        public int sizeX;
        public int sizeY;

        public string GetConfigurationPath()
        {
            return FileUtility.GetPath("markermaps/map_ar_bch_" + id + ".yml");
        }

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
            var mapOffsetX = surface.PixelToUnityCoord(sizeX) / 2f;
            var mapOffsetY = surface.PixelToUnityCoord(sizeY) / 2f;

            // origin of marker coordinates is top-left corner;
            var unityPosX = surface.PixelToUnityCoord(posX) + mapOffsetX;
            var unityPosY = 0f; // marker lies directly on display
            var unityPosZ = -(surface.PixelToUnityCoord(posY) + mapOffsetY);

            return new Vector3(unityPosX, unityPosY, unityPosZ);
        }
    }
}
