using Emgu.CV;
using Emgu.CV.Structure;
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
                while (keepRunning)
                {
                    var frame = capture.QueryFrame();
                    var image = frame.ToImage<Rgb, byte>();
                    currentImage = image.Bytes;
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
