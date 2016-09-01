
using System.Diagnostics;

namespace ImageProcessingTest
{
    class Init
    {
        static void JsonMsg(string msg)
        {
            //Debug.WriteLine(msg);
        }

        static void Main(string[] args)
        {
            //ImageProcessing.SetDummyCamera("C:/code/resources/dummy_image_default.png");
            ImageProcessing.SetOvrCamera(3, 0);
            ImageProcessing.StartImageProcessing();

            int pipeline = ImageProcessing.CreatePipeline();
            int output = ImageProcessing.AddOpenCvOutput(pipeline, "Test");
            int output2 = ImageProcessing.AddJsonOutput(pipeline, JsonMsg);
            int processor = ImageProcessing.AddArToolkitProcessor(pipeline, @"


		{
			""config"": {
				""calibration_left"": ""C:/code/resources/calib_ovrvision_left.dat"",
				""calibration_right"": ""C:/code/resources/calib_ovrvision_right.dat""
			},
			""markers"": [
				{
					""size"": 0.026,
					""pattern_path"": ""C:/code/resources/kanji.patt"",
					""type"": ""SINGLE"",
					""filter"": 5.0
				},
				{
					""size"": 0.026,
					""pattern_path"": ""C:/code/resources/hiro.patt"",
					""type"": ""SINGLE"",
					""filter"": 5.0
				}
			]
		}

");

            char keyPressed;
            int counter = 0;

            while (true)
            {
                ImageProcessing.ManualUpdate();
                keyPressed = (char)ImageProcessing.OpenCvWaitKey(5);

                if (keyPressed == 's')
                {
                    counter = (counter + 1) % 2;

                    switch (counter)
                    {
                        case 0:
                            //ImageProcessing.SetOvrCamera(2, 0);
                            ImageProcessing.SetDummyCamera("C:/code/resources/dummy2.jpg");
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
