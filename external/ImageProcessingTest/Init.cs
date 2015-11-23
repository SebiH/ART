using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageProcessingTest
{
    class Init
    {
        [DllImport("ImageProcessing")]
        private static extern IntPtr DetectMarker(IntPtr data, int width, int height);

        private static unsafe void TrackMarker(byte[] img, int width, int height)
        {
            fixed (byte* p = img)
            {
                IntPtr ptr = (IntPtr)p;
                // modifies the byte array that was passed in
                var result = DetectMarker(ptr, width, height);
            }
        }

        static void Main(string[] args)
        {
            using (var capture = new Capture(0))
            {
                while (true)
                {
                    // in
                    var frame = capture.QueryFrame();
                    var tempImg = new Image<Bgr, byte>(new Size(frame.Width, frame.Height));

                    var bytes = frame.GetData();
                    TrackMarker(bytes, frame.Width, frame.Height);

                    tempImg.Bytes = bytes;

                    // out
                    // unity uses RGB byte arrays, and c# methods don't switch channels in byte array!
                    var resultImg = new Image<Rgb, byte>(new Size(frame.Width, frame.Height));
                    CvInvoke.CvtColor(tempImg, resultImg, ColorConversion.Bgr2Rgb);

                    CvInvoke.Imshow("bla", resultImg);
                    CvInvoke.WaitKey(30);
                }
            }
        }
    }
}
