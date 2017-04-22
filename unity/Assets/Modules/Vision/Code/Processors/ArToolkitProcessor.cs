using System;
using System.IO;
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
            var globalDataPath = Path.Combine(Application.dataPath, "../../data");
            var config = new ArToolkitInitialSettings
            {
                calibration_left = Path.Combine(globalDataPath, "calib_ovrvision_left.dat"),
                calibration_right = Path.Combine(globalDataPath, "calib_ovrvision_right.dat")
            };

            _id = ImageProcessing.AddArToolkitProcessor(pipelineId, JsonUtility.ToJson(config));

            ImageProcessing.GetProcessorProperties(_registeredPipelineId, _id, GetPropertyCallback);
        }


        #region Processor properties

        [Serializable]
        private struct ArToolkitSettings
        {
            public float min_confidence;
            public float marker_size;
            public bool use_filters;
            public int max_missed_frames;
        }

        private ArToolkitSettings _currentSettings;
        public float MinConfidence
        {
            get { return _currentSettings.min_confidence; }
            set
            {
                _currentSettings.min_confidence = value;
                UpdateProperties();
            }
        }

        public float MarkerSize
        {
            get { return _currentSettings.marker_size; }
            set
            {
                _currentSettings.marker_size = value;
                UpdateProperties();
            }
        }

        public bool UseFilters
        {
            get { return _currentSettings.use_filters; }
            set
            {
                _currentSettings.use_filters = value;
                UpdateProperties();
            }
        }

        public int MaxMissedFrames
        {
            get { return _currentSettings.max_missed_frames; }
            set
            {
                _currentSettings.max_missed_frames = value;
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


        [Serializable]
        private struct ArToolkitInitialSettings
        {
            public string calibration_left;
            public string calibration_right;
        }
    }
}
