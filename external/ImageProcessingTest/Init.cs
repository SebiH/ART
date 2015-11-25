using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Diagnostics;
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
                capture.SetCaptureProperty(CapProp.FrameWidth, 1280);
                capture.SetCaptureProperty(CapProp.FrameHeight, 720);

                while (true)
                {
                    // in
                    var frame = capture.QueryFrame();
                    var watch = Stopwatch.StartNew();
                    var resultImg = new Image<Bgr, byte>(new Size(frame.Width, frame.Height));

                    var bytes = frame.GetData();
                    double[] pose = new double[6];
                    TrackMarker(bytes, frame.Width, frame.Height, pose);

                    resultImg.Bytes = bytes;

                    watch.Stop();
                    Console.Out.WriteLine(String.Format("{0,5:0.0}, {1,5:0.0}, {2,5:0.0}, {3,5:0.0}, {4,5:0.0}, {5,5:0.0}, took {6} ms", pose[0], pose[1], pose[2], pose[3], pose[4], pose[5], watch.ElapsedMilliseconds));

                    CvInvoke.Imshow("bla", resultImg);
                    CvInvoke.WaitKey(30);
                }
            }
        }
    }
}
