using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{
    class Init
    {
        [DllImport("ImageProcessing")]
        private static extern void StartImageProcessing();

        [DllImport("ImageProcessing")]
        private static extern int RegisterOpenCVTextureWriter(string modulename, string windowname);

        [DllImport("ImageProcessing")]
        private static extern void UpdateTextures();

        [DllImport("ImageProcessing")]
        private static extern int OpenCvWaitKey(int delay);

        static void Main(string[] args)
        {
            // Test things without unity
            StartImageProcessing();
            RegisterOpenCVTextureWriter("RawImage", "testWindow1");

            do
            {
                UpdateTextures();
            }
            while (OpenCvWaitKey(5) != 'q');
        }
    }
}
