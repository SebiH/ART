using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Assets
{
    class CameraImageProvider
    {
        [DllImport("ImageProcessing")]
        private static extern IntPtr DetectMarker(IntPtr data, int width, int height);



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


        private static void Run()
        {
            using (var capture = new Capture(0))
            {
                while (keepRunning)
                {
                    // in
                    var frame = capture.QueryFrame();
                    var resultImg = new Image<Bgr, byte>(new Size(640, 480));

                    // marshalling
                    byte[] bytes = resultImg.Bytes;
                    IntPtr inPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * bytes.Length);
                    Marshal.Copy(bytes, 0, inPtr, bytes.Length);

                    var resultPtr = DetectMarker(inPtr, frame.Width, frame.Height);

                    byte[] resultBytes = new byte[bytes.Length];
                    Marshal.Copy(resultPtr, resultBytes, 0, resultBytes.Length);

                    MemoryStream ms = new MemoryStream(resultBytes);
                    Bitmap tempBmp = new Bitmap(ms);
                    var x = new Image<Bgr, byte>(tempBmp);


                    // free
                    Marshal.FreeHGlobal(inPtr);
                    //Marshal.FreeHGlobal(resultPtr);

                    // out
                    // unity uses RGB byte arrays, and c# methods don't switch channels in byte array!
                    CvInvoke.CvtColor(x, resultImg, ColorConversion.Bgr2Rgb);

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
    }
}
