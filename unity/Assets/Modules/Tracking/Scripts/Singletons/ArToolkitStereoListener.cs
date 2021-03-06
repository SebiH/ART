using Assets.Modules.Core;
using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArToolkitStereoListener : ArMarkerTracker
    {
        public Pipeline ArToolkitPipeline;

        [Range(0, 1)]
        public float MinConfidence = 0.5f;
        public bool UseFilters = true;
        // for filter
        [Range(0, 1000)]
        public int MaxMissedFrames = 180;

        [Range(0f, 10f)]
        public float MaxMatchError = 2f;
        [Range(0f, 10f)]
        public float MaxTransformationError = 2f;

        private ArToolkitStereoProcessor _artkProcessor;
        private JsonOutput _artkOutput;

        private LockFreeQueue<ArToolkitOutput> _currentOutput = new LockFreeQueue<ArToolkitOutput>();

        public float Offset = -0.02f;

        public float CurrentMatchError = 0;
        public float CurrentTransError = 0;

        protected override void OnEnable()
        {
            ArMarkerTracker.Instance = this;
            Instance = this;

            _artkProcessor = new ArToolkitStereoProcessor();
            ArToolkitPipeline.AddProcessor(_artkProcessor);

            _artkOutput = new JsonOutput(OnPoseChanged);
            ArToolkitPipeline.AddOutput(_artkOutput);
        }

        void OnDisable()
        {
            ArToolkitPipeline.RemoveProcessor(_artkProcessor);
            ArToolkitPipeline.RemoveOutput(_artkOutput);
        }

        private void OnPoseChanged(string json_msg)
        {
            try
            {
                _currentOutput.Enqueue(JsonUtility.FromJson<ArToolkitOutput>(json_msg));
            }
            catch
            {
                Debug.LogError("Error deserializing message: \n" + json_msg);
            }
        }



        void Update()
        {
            ArToolkitOutput output;
            if (_currentOutput.Dequeue(out output))
            {
                // only fetch latest output
                var latestOutput = output;
                while (_currentOutput.Dequeue(out latestOutput))
                {
                    output = latestOutput;
                }

                CurrentTransError = float.MaxValue;
                CurrentMatchError = float.MaxValue;

                foreach (var marker in output.markers)
                {
                    ProcessMarker(marker);
                }
            }

            if (MinConfidence != _artkProcessor.MinConfidence)
            {
                _artkProcessor.MinConfidence = MinConfidence;
                // TODO: workaround since minconfidence will be propagated to c++ lib, may limit value
                MinConfidence = _artkProcessor.MinConfidence;
            }

            if (UseFilters != _artkProcessor.UseFilters)
            {
                _artkProcessor.UseFilters = UseFilters;
                // TODO: workaround since minconfidence will be propagated to c++ lib, may limit value
                UseFilters = _artkProcessor.UseFilters;
            }

            if (MaxMissedFrames != _artkProcessor.MaxMissedFrames)
            {
                _artkProcessor.MaxMissedFrames = MaxMissedFrames;
                // TODO: workaround since minconfidence will be propagated to c++ lib, may limit value
                MaxMissedFrames = _artkProcessor.MaxMissedFrames;
            }
        }


        private void ProcessMarker(MarkerInfo marker)
        {
            CurrentMatchError = Mathf.Min(marker.match_error, CurrentMatchError);
            CurrentTransError = Mathf.Min(marker.trans_error, CurrentTransError);

            if (marker.match_error < MaxMatchError && marker.trans_error < MaxTransformationError)
            {
                Matrix4x4 matrixRaw = MatrixFromFloatArray(marker.transformation_matrix);
                var transformMatrix = LHMatrixFromRHMatrix(matrixRaw);

                OnNewPoseDetected(new MarkerPose
                {
                    Id = marker.id,
                    Confidence = marker.confidence,
                    Position = transformMatrix.GetPosition() + new Vector3(Offset, 0, 0),
                    Rotation = transformMatrix.GetRotation()
                });
            }
        }

        // Taken from ARUtilityFunctions.cs in artoolkit/arunity5
        private Matrix4x4 LHMatrixFromRHMatrix(Matrix4x4 rhm)
        {
            Matrix4x4 lhm = new Matrix4x4(); ;

            // Column 0.
            lhm[0, 0] = rhm[0, 0];
            lhm[1, 0] = rhm[1, 0];
            lhm[2, 0] = -rhm[2, 0];
            lhm[3, 0] = rhm[3, 0];

            // Column 1.
            lhm[0, 1] = rhm[0, 1];
            lhm[1, 1] = rhm[1, 1];
            lhm[2, 1] = -rhm[2, 1];
            lhm[3, 1] = rhm[3, 1];

            // Column 2.
            lhm[0, 2] = -rhm[0, 2];
            lhm[1, 2] = -rhm[1, 2];
            lhm[2, 2] = rhm[2, 2];
            lhm[3, 2] = -rhm[3, 2];

            // Column 3.
            lhm[0, 3] = rhm[0, 3];
            lhm[1, 3] = rhm[1, 3];
            lhm[2, 3] = -rhm[2, 3];
            lhm[3, 3] = rhm[3, 3];

            return lhm;
        }


        // Taken from ARUtilityFunctions.cs in artoolkit/arunity5
        private Matrix4x4 MatrixFromFloatArray(float[] values)
        {
            if (values == null || values.Length < 16) throw new ArgumentException("Expected 16 elements in values array", "values");

            Matrix4x4 mat = new Matrix4x4();
            for (int i = 0; i < 16; i++) mat[i] = values[i];
            return mat;
        }

        protected override void UpdateMarkerSize(float size)
        {
            // artoolkit uses mm, size is given in cm
            _artkProcessor.MarkerSize = size * 1000;
        }

        public override float GetMinConfidence()
        {
            return MinConfidence;
        }


        #region JSON Message Content
        [Serializable]
        private struct MarkerInfo
        {
            public int id;
            public float confidence;
            public float[] transformation_matrix;
            public float match_error;
            public float trans_error;
        }

        [Serializable]
        private struct ArToolkitOutput
        {
            public MarkerInfo[] markers;
        }
        #endregion
    }
}
