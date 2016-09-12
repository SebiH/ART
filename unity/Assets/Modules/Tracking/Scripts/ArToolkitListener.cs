using Assets.Modules.Core.Util;
using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArToolkitListener : MonoBehaviour
    {
        #region JSON Message Content
        [Serializable]
        private class PoseMatrix
        {
            public float m00, m01, m02, m03;
            public float m10, m11, m12, m13;
            public float m20, m21, m22, m23;
        }

        [Serializable]
        private class Corners
        {
            public double[] topleft;
            public double[] topright;
            public double[] bottomleft;
            public double[] bottomright;
        }

        [Serializable]
        private class MarkerInfo
        {
            public int id;
            public string name;
            public double[] pos;
            public Corners corners;
            public PoseMatrix transform_matrix;
        }

        [Serializable]
        private class ArToolkitOutput
        {
            public MarkerInfo[] markers_left;
            public MarkerInfo[] markers_right;
        }
        #endregion

        public static ArToolkitListener Instance;

        public Pipeline ArToolkitPipeline;

        public delegate void NewPoseHandler(MarkerPose pose);
        public event NewPoseHandler NewPoseDetected;

        private ArToolkitProcessor _artkProcessor;
        private JsonOutput _artkOutput;

        private bool _hasNewOutput = false;
        private ArToolkitOutput _currentOutput;

        void OnEnable()
        {
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

        void OnPoseChanged(string json_msg)
        {
            try
            {
                _currentOutput = JsonUtility.FromJson<ArToolkitOutput>(json_msg);
                _hasNewOutput = _currentOutput != null;
            }
            catch
            {
                Debug.LogError("Error deserializing message: \n" + json_msg);
            }
        }



        void Update()
        {
            if (_hasNewOutput)
            {
                _hasNewOutput = false;
                var output = _currentOutput;

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

            transformMatrix = transformMatrix.inverse;


            if (NewPoseDetected != null)
            {
                NewPoseDetected(new MarkerPose(marker.id, marker.name, transformMatrix));
            }
        }
    }
}
