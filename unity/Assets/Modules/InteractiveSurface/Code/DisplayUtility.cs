using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public static class DisplayUtility
    {
        public static float PixelToCmRatio = 1;

        public static float PixelToUnityCoord(int pixelLocation)
        {
            return PixelToCmRatio * pixelLocation;           
        }

        public static int UnityToPixelLocation(float unityCoord)
        {
            return Mathf.RoundToInt((1 / PixelToCmRatio) * unityCoord);
        }
    }
}
