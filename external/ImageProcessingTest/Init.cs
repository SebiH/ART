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
        private static extern IntPtr DetectMarker(IntPtr data, int width, int height, IntPtr pose);

        private static unsafe void TrackMarker(byte[] img, int width, int height, double[] pose)
        {
            fixed (double* posePtr = pose)
            fixed (byte* imgPtr = img)
            {
                IntPtr rawImgPtr = (IntPtr)imgPtr;
                IntPtr rawPosePtr = (IntPtr)posePtr;
                var result = DetectMarker(rawImgPtr, width, height, rawPosePtr);
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
                    double[] pose = new double[6];
                    TrackMarker(bytes, frame.Width, frame.Height, pose);

                    tempImg.Bytes = bytes;
                    Console.Out.WriteLine(String.Format("{0}, {1}, {2}", pose[0], pose[1], pose[2]));

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
