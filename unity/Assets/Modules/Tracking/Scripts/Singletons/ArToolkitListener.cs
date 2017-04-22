using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using System.Collections;
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
        [Range(0, 1000)]
        public int MaxMissedFrames = 180;

        private ArToolkitProcessor _artkProcessor;
        private JsonOutput _artkOutput;

        private Queue _currentOutput;
        private readonly Quaternion _rotationAdjustment = Quaternion.Euler(90, 0, 0);

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

                foreach (var marker in output.markers_left)
                {
                    ProcessMarker(marker);
                }

                // TODO: combine, in case marker shows up in both?
                //foreach (var marker in _currentOutput.markers_right)
                //{
                //    ProcessMarker(marker);
                //}
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
        }


        private void ProcessMarker(MarkerInfo marker)
        {
            var pose = marker.transform_matrix;
            var transformMatrix = new Matrix4x4();

            transformMatrix.m00 = pose.m00;
            transformMatrix.m01 = pose.m01;
            transformMatrix.m02 = pose.m02;
            transformMatrix.m03 = pose.m03;

            transformMatrix.m10 = pose.m10;
            transformMatrix.m11 = pose.m11;
            transformMatrix.m12 = pose.m12;
            transformMatrix.m13 = pose.m13;

            transformMatrix.m20 = pose.m20;
            transformMatrix.m21 = pose.m21;
            transformMatrix.m22 = pose.m22;
            transformMatrix.m23 = pose.m23;

            transformMatrix.m30 = 0;
            transformMatrix.m31 = 0;
            transformMatrix.m32 = 0;
            transformMatrix.m33 = 1;

            var pos = transformMatrix.GetPosition();
            pos.y = -pos.y;
            var rot = transformMatrix.GetRotation();
            rot = rot * _rotationAdjustment;

            OnNewPoseDetected(new MarkerPose
            {
                Id = marker.id,
                Confidence = marker.confidence,
                Position = pos,
                Rotation = rot
            });
        }

        protected override void UpdateMarkerSize(float size)
        {
            _artkProcessor.MarkerSize = size;
        }




        #region JSON Message Content
        [Serializable]
        private struct PoseMatrix
        {
            public float m00, m01, m02, m03;
            public float m10, m11, m12, m13;
            public float m20, m21, m22, m23;
        }

        [Serializable]
        private struct Corners
        {
            public double[] topleft;
            public double[] topright;
            public double[] bottomleft;
            public double[] bottomright;
        }

        [Serializable]
        private struct MarkerInfo
        {
            public int id;
            public double confidence;
            public double[] pos;
            public Corners corners;
            public PoseMatrix transform_matrix;
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
