
using System;
using System.Runtime.InteropServices;

namespace ImageProcessingTest
{
    class Init
    {
        [DllImport("ImageProcessing")]
        private static extern IntPtr DetectMarker(IntPtr data, int width, int height);

        private static unsafe void Test()
        {
            byte[] buffer = { 1, 2, 3 };

            fixed (byte* p = buffer)
            {
                IntPtr ptr = (IntPtr)p;
                var result = DetectMarker(ptr, 10, 10);
            }
        }

        static void Main(string[] args)
        {
            Test();
        }
    }
}
