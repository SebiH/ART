
namespace ImageProcessingTest
{
    class Init
    {
        static void Main(string[] args)
        {
            ImageProcessing.SetDummyCamera("C:/code/resources/dummy_image_default.png");
            ImageProcessing.StartImageProcessing();

            int pipeline = ImageProcessing.CreatePipeline();
            int output = ImageProcessing.AddOpenCvOutput(pipeline, "Test");

            char keyPressed;

            while (true)
            {
                keyPressed = (char)ImageProcessing.OpenCvWaitKey(5);

                if (keyPressed == 'q')
                {
                    ImageProcessing.RemoveOutput(pipeline, output);
                    ImageProcessing.RemovePipeline(pipeline);
                    ImageProcessing.StopImageProcessing();
                    break;
                }
            }
        }
    }
}
