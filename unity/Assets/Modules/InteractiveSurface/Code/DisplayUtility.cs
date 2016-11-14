using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public static class DisplayUtility
    {
        public static float PixelToCmRatio = 0.01f;

        public static float PixelToUnityCoord(float pixelLocation)
        {
            // pixel coords uses cm, unity uses m
            return PixelToCmRatio * pixelLocation / 100f;           
        }

        public static float UnityToPixelLocation(float unityCoord)
        {
            // pixel coords uses cm, unity uses m
            return (1 / PixelToCmRatio) * unityCoord * 100f;
        }
    }
}
