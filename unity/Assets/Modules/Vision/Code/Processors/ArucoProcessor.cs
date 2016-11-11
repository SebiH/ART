using System;
using UnityEngine;

namespace Assets.Modules.Vision.Processors
{
    public class ArucoProcessor : IProcessor
    {
        private int _registeredPipelineId = -1;
        private int _id = -1;

        public float MarkerSizeInMeter;
        public bool UseTracker = false;
        public float TrackerErrorRatio = 4.0f;

        public ArucoProcessor(float markerSizeInMeter)
        {
            MarkerSizeInMeter = markerSizeInMeter;
        }

        public void Register(int pipelineId)
        {
            _registeredPipelineId = pipelineId;
            // Double braces in json due to String.Format
            _id = ImageProcessing.AddArucoProcessor(pipelineId, GetJsonProperties());

            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveProcessor(pipelineId, _id);
            _id = -1;
            _registeredPipelineId = -1;
        }


        #region Processor Properties

        private string GetJsonProperties()
        {
            return String.Format(@"
                {{
                    ""marker_size_m"": {0},
                    ""use_tracker"": {1},
                    ""tracker_error_ratio"": {2}
                }}", MarkerSizeInMeter, UseTracker.ToString().ToLower(), TrackerErrorRatio);
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

        #endregion
    }
}
