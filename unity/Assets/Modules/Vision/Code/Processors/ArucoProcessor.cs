using System;
using UnityEngine;

namespace Assets.Modules.Vision.Processors
{
    public class ArucoProcessor : IProcessor
    {
        private int _registeredPipelineId = -1;
        private int _id = -1;

        private double _markerSizeInMeter;

        public ArucoProcessor(double markerSizeInMeter)
        {
            _markerSizeInMeter = markerSizeInMeter;
        }

        public void Register(int pipelineId)
        {
            _registeredPipelineId = pipelineId;
            // Double braces in json due to String.Format
            _id = ImageProcessing.AddArucoProcessor(pipelineId, String.Format(@"
                {{
                    ""marker_size_m"": {0}
                }}", _markerSizeInMeter));

            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveProcessor(pipelineId, _id);
            _id = -1;
            _registeredPipelineId = -1;
        }


        #region Processor Properties

        // TODO.

        public void UpdateProperties()
        {
            //ImageProcessing.SetProcessorProperties(_registeredPipelineId, _id, JsonUtility.ToJson())
            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }

        private void GetPropertyCallback(string json_properties_str)
        {
            // TODO.
        }

        #endregion
    }
}
