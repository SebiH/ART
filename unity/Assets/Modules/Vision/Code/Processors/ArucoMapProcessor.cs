using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Vision.Processors
{
    public class ArucoMapProcessor : IProcessor
    {
        private int _registeredPipelineId = -1;
        private int _id = -1;

        public float MarkerSizeInMeter;
        public float TrackerErrorRatio = 4.0f;
        public readonly List<Map> Maps = new List<Map>();

        public ArucoMapProcessor(float markerSizeInMeter)
        {
            MarkerSizeInMeter = markerSizeInMeter;
        }

        public void Register(int pipelineId)
        {
            _registeredPipelineId = pipelineId;
            _id = ImageProcessing.AddArucoMapProcessor(pipelineId, GetJsonProperties());
            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }

        public void Deregister(int pipelineId)
        {
            ImageProcessing.RemoveProcessor(pipelineId, _id);
            _id = -1;
            _registeredPipelineId = -1;
        }


        #region Processor Properties

        [Serializable]
        public struct Map
        {
            public int id;
            public string path;
            public float marker_size_m;
        }

        [Serializable]
        private struct Params
        {
            public float tracker_error_ratio;
            public Map[] maps;
        }

        private string GetJsonProperties()
        {
            var settings = new Params
            {
                tracker_error_ratio = TrackerErrorRatio,
                maps = Maps.ToArray()
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

        #endregion
    }
}
