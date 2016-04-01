using System;
using System.Runtime.InteropServices;

namespace Assets.Scripts.Camera
{
    class ImageProcessing
    {
        [DllImport("ImageProcessing")]
        public static extern void OvrStart(int cameraMode = -1);

        [DllImport("ImageProcessing")]
        public static extern void OvrStop();

        [DllImport("ImageProcessing")]
        public static extern void WriteROITexture(int startX, int startY, int width, int height, IntPtr leftUnityPtr, IntPtr rightUnityPtr);

        [DllImport("ImageProcessing")]
        public static extern float GetProperty(string prop);

        [DllImport("ImageProcessing")]
        public static extern void SetProperty(string prop, float val);

        [DllImport("ImageProcessing")]
        public static extern void WriteTexture(IntPtr leftUnityPtr, IntPtr rightUnityPtr);
    }
}
