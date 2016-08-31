using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.Processors
{
    public class ArToolkitProcessor : IProcessor
    {
        private int _id = -1;

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveProcessor(pipelineId, _id);
        }

        public void Register(int pipelineId)
        {
            _id = ImageProcessing.AddArToolkitProcessor(pipelineId, @"
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
                }");
            _id = -1;
        }
    }
}
