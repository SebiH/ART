
using System.Diagnostics;

namespace ImageProcessingTest
{
    class Init
    {
        static void JsonMsg(string msg)
        {
            Debug.WriteLine(msg);
        }

        static void Main(string[] args)
        {
            //ImageProcessing.SetDummyCamera("C:/code/resources/dummy_image_default.png");
            ImageProcessing.SetOvrCamera(2, 0);
            ImageProcessing.StartImageProcessing();

            int pipeline = ImageProcessing.CreatePipeline();
            int output = ImageProcessing.AddOpenCvOutput(pipeline, "Test");
            int output2 = ImageProcessing.AddJsonOutput(pipeline, JsonMsg);
            int processor = ImageProcessing.AddArToolkitProcessor(pipeline);

            char keyPressed;
            int counter = 0;

            while (true)
            {
                keyPressed = (char)ImageProcessing.OpenCvWaitKey(5);

                if (keyPressed == 's')
                {
                    counter = (counter + 1) % 2;

                    switch (counter)
                    {
                        case 0:
                            ImageProcessing.SetOvrCamera(2, 0);
                            break;
                        case 1:
                            ImageProcessing.SetDummyCamera("C:/code/resources/dummy3.jpg");
                            break;
                    }
                }

                if (keyPressed == 'q')
                {
                    ImageProcessing.RemoveProcessor(pipeline, processor);
                    ImageProcessing.RemoveOutput(pipeline, output);
                    ImageProcessing.RemoveOutput(pipeline, output2);
                    ImageProcessing.RemovePipeline(pipeline);
                    ImageProcessing.StopImageProcessing();
                    break;
                }
            }
        }
    }
}
