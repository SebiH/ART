using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{
    class Init
    {
        [DllImport("ImageProcessing")]
        private static extern void StartImageProcessing();

        [DllImport("ImageProcessing")]
        private static extern int RegisterOpenCVTextureWriter(string modulename, string windowname);

        [DllImport("ImageProcessing")]
        private static extern void DeregisterTexturePtr(int handle);

        [DllImport("ImageProcessing")]
        private static extern void UpdateTextures();

        [DllImport("ImageProcessing")]
        private static extern int OpenCvWaitKey(int delay);

        [DllImport("ImageProcessing")]
        private static extern void ChangeRoi(int moduleHandle, int x, int y, int width, int height);

        [DllImport("ImageProcessing")]
        private static extern float GetCameraProperty(string propName);

        static void Main(string[] args)
        {
            // Test things without unity
            StartImageProcessing();
            int handleRaw = RegisterOpenCVTextureWriter("RawImage", "testWindow1");
            int handleRoi = RegisterOpenCVTextureWriter("ROI", "testWindow2");

            int currentX = 0;
            int currentY = 0;
            int currentWidth = (int)GetCameraProperty("width");
            int currentHeight = (int)GetCameraProperty("height");
            ChangeRoi(-1, currentX, currentY, currentWidth, currentHeight);

            char keyPressed;

            do
            {
                UpdateTextures();
                keyPressed = (char)OpenCvWaitKey(5);

                if (keyPressed == 'r')
                {
                    ChangeRoi(-1, currentX, currentY, currentWidth, currentHeight);
                    currentX += 10;
                    currentY += 10;
                    currentWidth -= 10;
                    currentHeight -= 10;

                    if (currentWidth < 50 || currentHeight < 50)
                    {
                        currentX = 0;
                        currentY = 0;
                        currentWidth = (int)GetCameraProperty("width");
                        currentHeight = (int)GetCameraProperty("height");
                    }
                }

                if (keyPressed == 'x')
                {
                    if (handleRaw == -1)
                    {
                        handleRaw = RegisterOpenCVTextureWriter("RawImage", "testWindow1");
                    }
                    else
                    {
                        DeregisterTexturePtr(handleRaw);
                        handleRaw = -1;
                    }
                }
            }
            while (keyPressed != 'q');
        }
    }
}
