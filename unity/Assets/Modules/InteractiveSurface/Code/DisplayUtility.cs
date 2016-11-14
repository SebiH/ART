using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public static class DisplayUtility
    {
        public static float PixelToCmRatio = 1;

        public static float PixelToUnityCoord(int pixelLocation)
        {
            // pixel coords uses cm, unity uses m
            return PixelToCmRatio * pixelLocation * 100;           
        }

        public static int UnityToPixelLocation(float unityCoord)
        {
            // pixel coords uses cm, unity uses m
            return Mathf.RoundToInt((1 / PixelToCmRatio) * (unityCoord / 100f));
        }
    }
}
