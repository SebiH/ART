using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{
    class Init
    {
        [DllImport("ImageProcessing")]
        private static extern void SetFrameSource(int frameSourceId);

        [DllImport("ImageProcessing")]
        private static extern void StartImageProcessing();

        [DllImport("ImageProcessing")]
        private static extern void StopImageProcessing();

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
        private static extern int GetCamWidth();

        [DllImport("ImageProcessing")]
        private static extern int GetCamHeight();

        [DllImport("ImageProcessing")]
        private static extern int GetCamChannels();

        [DllImport("ImageProcessing")]
        private static extern float GetCamGain();

        [DllImport("ImageProcessing")]
        private static extern void SetCamGain(float val);

        [DllImport("ImageProcessing")]
        private static extern float GetCamExposure();

        [DllImport("ImageProcessing")]
        private static extern void SetCamExposure(float val);


        [DllImport("ImageProcessing")]
        private static extern void SetProcessingMode(int mode);

        [DllImport("ImageProcessing")]
        private static extern int GetProcessingMode();

        private enum FrameSource
        {
            None = -1,
            OpenCV = 0,
            Ovr1280x960x45fps = 4,
            Ovr1280x800x60fps = 6,
            Ovr320x240x120fps = 8
        }

        static void Main(string[] args)
        {
            // Test things without unity
            SetFrameSource((int)FrameSource.OpenCV);
            StartImageProcessing();
            SetCamExposure(100);
            SetCamGain(1);
            int handleRaw = RegisterOpenCVTextureWriter("RawImage", "testWindow1");
            int handleRoi = RegisterOpenCVTextureWriter("ROI", "testWindow2");

            int currentX = 0;
            int currentY = 0;
            int currentWidth = GetCamWidth();
            int currentHeight = GetCamHeight();
            ChangeRoi(-1, currentX, currentY, currentWidth, currentHeight);

            char keyPressed;

            do
            {
                UpdateTextures();
                keyPressed = (char)OpenCvWaitKey(5);

                if (keyPressed == 'p')
                {
                    SetProcessingMode((GetProcessingMode() + 1) % 3);
                }

                if (keyPressed == 's')
                {
                    StopImageProcessing();
                    break;
                }

                if (keyPressed == 'r')
                {
                    ChangeRoi(-1, currentX, currentY, currentWidth, currentHeight);
                    currentX += 10;
                    currentY += 10;
                    currentWidth -= 20;
                    currentHeight -= 20;

                    if (currentWidth < 50 || currentHeight < 50)
                    {
                        currentX = 0;
                        currentY = 0;
                        currentWidth = GetCamWidth();
                        currentHeight = GetCamHeight();
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
