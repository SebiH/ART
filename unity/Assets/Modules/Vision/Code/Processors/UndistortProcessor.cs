using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Vision.Processors
{
    public class UndistortProcessor : IProcessor
    {
        private int _registeredPipelineId = -1;
        private int _id = -1;

        public void Register(int pipelineId)
        {
            _registeredPipelineId = pipelineId;
            _id = ImageProcessing.AddUndistortProcessor(pipelineId, GetJsonProperties());

            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveProcessor(pipelineId, _id);
            _id = -1;
            _registeredPipelineId = -1;
        }

        private string GetJsonProperties()
        {
            var globalDataPath = Path.Combine(Application.dataPath, "../../data");

            var settings = new UndistortSettings
            {
                intrinsic_left = Path.Combine(globalDataPath, "calib_gopro_standard_left_intrinsic.yaml"),
                distcoeffs_left = Path.Combine(globalDataPath, "calib_gopro_standard_left_distcoeffs.yaml"),

                intrinsic_right = Path.Combine(globalDataPath, "calib_gopro_standard_right_intrinsic.yaml"),
                distcoeffs_right = Path.Combine(globalDataPath, "calib_gopro_standard_right_distcoeffs.yaml")
            };
            return JsonUtility.ToJson(settings);
        }

        public void UpdateProperties()
        {
            ImageProcessing.SetProcessorProperties(_registeredPipelineId, _id, GetJsonProperties());
            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }

        private void GetPropertyCallback(string json_properties_str)
        {
            // TODO.
        }

        [Serializable]
        private struct UndistortSettings
        {
            public string intrinsic_left;
            public string distcoeffs_left;

            public string intrinsic_right;
            public string distcoeffs_right;
        }
    }
}
