using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{
    class Init
    {
        [DllImport("ImageProcessing")]
        private static extern void StereoCalibration();

        [DllImport("ImageProcessing")]
        private static extern void StereoDetection();

        static void Main(string[] args)
        {
            // Test things without unity
            StereoDetection();
        }
    }
}
