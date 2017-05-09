using Assets.Modules.Core;
using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArToolkitListener : ArMarkerTracker
    {
        public Pipeline ArToolkitPipeline;

        [Range(0, 1)]
        public float MinConfidence = 0.5f;
        public bool UseFilters = true;
        // for filter
        [Range(0, 200)]
        public int MaxMissedFrames = 10;

        [Range(-0.05f, 0.05f)]
        public float OffsetLeft = 0;
        [Range(-0.05f, 0.05f)]
        public float OffsetRight = 0.0032f;

        private ArToolkitProcessor _artkProcessor;
        private JsonOutput _artkOutput;

        private Queue _currentOutput;

        public ArToolkitListener()
        {
            _currentOutput = Queue.Synchronized(new Queue());
        }

        protected override void OnEnable()
        {
            ArMarkerTracker.Instance = this;
            Instance = this;

            _artkProcessor = new ArToolkitProcessor();
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
            if (_currentOutput.Count > 0)
            {
                // only fetch latest output
                ArToolkitOutput output = (ArToolkitOutput)_currentOutput.Dequeue();
                while (_currentOutput.Count > 0)
                {
                    output = (ArToolkitOutput)_currentOutput.Dequeue();
                }

                var pairs = new List<MarkerPair>();

                foreach (var marker in output.markers_left)
                {
                    pairs.Add(new MarkerPair { Left = marker });
                }

                foreach (var marker in output.markers_right)
                {
                    var match = pairs.FirstOrDefault(p => p.Left.id == marker.id);
                    if (match != null)
                    {
                        match.Right = marker;
                    }
                    else
                    {
                        pairs.Add(new MarkerPair { Right = marker });
                    }
                }

                foreach (var pair in pairs)
                {
                    ProcessMarker(pair);
                }
            }

            if (MinConfidence != _artkProcessor.MinConfidence)
            {
                _artkProcessor.MinConfidence = MinConfidence;
                // TODO: workaround since property will be propagated to c++ lib, may limit value
                MinConfidence = _artkProcessor.MinConfidence;
            }

            if (UseFilters != _artkProcessor.UseFilters)
            {
                _artkProcessor.UseFilters = UseFilters;
                // TODO: workaround since property will be propagated to c++ lib, may limit value
                UseFilters = _artkProcessor.UseFilters;
            }

            if (MaxMissedFrames != _artkProcessor.MaxMissedFrames)
            {
                _artkProcessor.MaxMissedFrames = MaxMissedFrames;
                // TODO: workaround since property will be propagated to c++ lib, may limit value
                MaxMissedFrames = _artkProcessor.MaxMissedFrames;
            }
        }


        private void ProcessMarker(MarkerPair marker)
        {
            var positions = new List<Vector3>();
            var rotations = new List<Quaternion>();
            int id = -1;
            float confidence = 0;

            if (marker.Left != null)
            {
                var matrixRaw = MatrixFromFloatArray(marker.Left.transformation_matrix);
                var transformMatrix = LHMatrixFromRHMatrix(matrixRaw);
                positions.Add(transformMatrix.GetPosition() + new Vector3(OffsetLeft, 0, 0));
                rotations.Add(transformMatrix.GetRotation());
                id = marker.Left.id;
                confidence = Mathf.Max(confidence, marker.Left.confidence);
            }

            if (marker.Right != null)
            {
                var matrixRaw = MatrixFromFloatArray(marker.Right.transformation_matrix);
                var transformMatrix = LHMatrixFromRHMatrix(matrixRaw);
                positions.Add(transformMatrix.GetPosition() + new Vector3(OffsetRight, 0, 0));
                rotations.Add(transformMatrix.GetRotation());
                id = marker.Right.id;
                confidence = Mathf.Max(confidence, marker.Right.confidence);
            }

            OnNewPoseDetected(new MarkerPose
            {
                Id = id,
                Confidence = confidence,
                Position = MathUtility.Average(positions),
                Rotation = MathUtility.Average(rotations)
            });
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
            // artoolkit uses mm, size is given in m
            _artkProcessor.MarkerSize = size * 1000;
        }

        public override float GetMinConfidence()
        {
            return MinConfidence;
        }


        private class MarkerPair
        {
            public MarkerInfo Left = null;
            public MarkerInfo Right = null;
        }

        #region JSON Message Content
        [Serializable]
        private class MarkerInfo
        {
            public int id = -1;
            public float confidence = 0;
            public float match_error = 0;
            public float trans_error = 0;
            public float[] transformation_matrix = null;
        }

        [Serializable]
        private struct ArToolkitOutput
        {
            public MarkerInfo[] markers_left;
            public MarkerInfo[] markers_right;
        }
        #endregion
    }
}
