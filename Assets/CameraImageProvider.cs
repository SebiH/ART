using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace Assets
{
    class CameraImageProvider
    {
        [DllImport("ImageProcessing")]
        private static extern IntPtr DetectMarker(IntPtr data, int width, int height, IntPtr pose);



        private static bool isRunning;
        private static bool keepRunning = true;

        public static void Init()
        {
            if (!isRunning)
            {
                isRunning = true;
                Thread t = new Thread(new ThreadStart(Run));
                t.Start();
            }

        }

        public static void Stop()
        {
            keepRunning = false;
            isRunning = false;
        }



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

        private static void Run()
        {
            using (var capture = new Capture(0))
            {
                capture.SetCaptureProperty(CapProp.FrameWidth, 1280);
                capture.SetCaptureProperty(CapProp.FrameHeight, 720);

                while (keepRunning)
                {
                    // in
                    var frame = capture.QueryFrame();
                    var tempImg = new Image<Bgr, byte>(new Size(frame.Width, frame.Height));

                    var bytes = frame.GetData();
                    TrackMarker(bytes, frame.Width, frame.Height, pose);

                    tempImg.Bytes = bytes;

                    // out
                    // unity uses RGB byte arrays, and c# methods don't switch channels in byte array!
                    var resultImg = new Image<Rgb, byte>(new Size(frame.Width, frame.Height));
                    CvInvoke.CvtColor(tempImg, resultImg, ColorConversion.Bgr2Rgb);

                    currentImage = resultImg.Bytes;
                    imageGeneration++;
                }
            }
        }

        private static byte[] currentImage;

        public static byte[] getCurrentImage()
        {
            return currentImage;
        }

        private static int imageGeneration;

        public static int GetImageGeneration()
        {
            return imageGeneration;
        }


        private static double[] pose = new double[6];
        public static double[] GetCurrentPose()
        {
            return pose;
        }
    }
}
