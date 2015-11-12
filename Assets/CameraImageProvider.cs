using Emgu.CV;
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
        }

        private static void Run()
        {
            var capture = new Capture(0);
            var i = 0;

            while (keepRunning)
            {
                if (i > 3000)
                    keepRunning = false;

                var frame = capture.QueryFrame();
                currentImage = frame.Bytes;
                imageGeneration++;
            }
        }


        private static byte[] currentImage = null;

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
