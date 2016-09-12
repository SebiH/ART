using System;
using UnityEngine;

namespace Assets.Modules.Vision.Processors
{
    public class ArToolkitProcessor : IProcessor
    {
        private int _registeredPipelineId = -1;
        private int _id = -1;

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveProcessor(pipelineId, _id);
            _id = -1;
            _registeredPipelineId = -1;
        }

        public void Register(int pipelineId)
        {
            _registeredPipelineId = pipelineId;
            _id = ImageProcessing.AddArToolkitProcessor(pipelineId, @"
                {
                    ""config"": {
                        ""calibration_left"": ""C:/code/resources/calib_ovrvision_left.dat"",
                        ""calibration_right"": ""C:/code/resources/calib_ovrvision_right.dat""
                    },
                    ""markers"": [
                        {
                            ""size"": 0.026,
                            ""name"": ""kanji"",
                            ""pattern_path"": ""C:/code/resources/kanji.patt"",
                            ""type"": ""SINGLE"",
                            ""filter"": 5.0
                        },
                        {
                            ""size"": 0.026,
                            ""name"": ""hiro"",
                            ""pattern_path"": ""C:/code/resources/hiro.patt"",
                            ""type"": ""SINGLE"",
                            ""filter"": 5.0
                        }
                    ]
                }");

            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }


        #region Processor properties

        [Serializable]
        private struct ArToolkitSettings
        {
            public float min_confidence;
        }

        private ArToolkitSettings _currentSettings;
        public float MinConfidence
        {
            get
            {
                return _currentSettings.min_confidence;
            }
            set
            {
                _currentSettings.min_confidence = value;
                UpdateProperties();
            }
        }



        public void UpdateProperties()
        {
            ImageProcessing.SetProcessorProperties(_registeredPipelineId, _id, JsonUtility.ToJson(_currentSettings));
            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }

        private void GetPropertyCallback(string json_properties_str)
        {
            _currentSettings = JsonUtility.FromJson<ArToolkitSettings>(json_properties_str);
        }

        #endregion
    }
}
