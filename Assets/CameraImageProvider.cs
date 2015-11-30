using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ImageProcessingUtil;
using System.Drawing;
using System.Threading;

namespace Assets
{
    class CameraImageProvider
    {
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
                capture.SetCaptureProperty(CapProp.FrameWidth, 1280);
                capture.SetCaptureProperty(CapProp.FrameHeight, 720);

                while (keepRunning)
                {
                    // in
                    var frame = capture.QueryFrame();

                    pose = new MarshalledPose();
                    var tempImg = frame.ToImage<Bgr, byte>();
                    ImageProcessor.TrackMarker(ref tempImg, ref pose);

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


        private static MarshalledPose pose;
        public static MarshalledPose GetCurrentPose()
        {
            return pose;
        }
    }
}
