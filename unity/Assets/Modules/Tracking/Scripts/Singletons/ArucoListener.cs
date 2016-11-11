using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArucoListener : MonoBehaviour
    {
        #region JSON message content

        [Serializable]
        private struct Vec3_
        {
            public float x;
            public float y;
            public float z;

            public Vector3 ToUnityVec3()
            {
                return new Vector3(x, -y, z);
            }
        }


        [Serializable]
        private struct Quat_
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public Quaternion ToUnityQuaternion()
            {
                return new Quaternion(x, y, z, w);
            }
        }

        [Serializable]
        private struct ArucoPose
        {
            public int id;
            public Vec3_ position;
            public Quat_ rotation;
        }

        [Serializable]
        private struct ArucoOutput
        {
            public ArucoPose[] markers_left;
            public ArucoPose[] markers_right;
        }

        #endregion

        public static ArucoListener Instance;

        // Cannot be changed once script is running
        public Pipeline ArucoPipeline;
        // Cannot be changed once script is running
        public float MarkerSizeInMeter = 0.15f;
        public bool UseTracker = false;

        public readonly Dictionary<int, MarkerPose> DetectedPoses;

        public delegate void NewPoseHandler(MarkerPose pose);
        public event NewPoseHandler NewPoseDetected;

        private ArucoProcessor _arProcessor;
        private JsonOutput _arOutput;

        private bool _hasNewOutput = false;
        private ArucoOutput _currentOutput;

        void OnEnable()
        {
            Instance = this;

            _arProcessor = new ArucoProcessor(MarkerSizeInMeter, UseTracker);
            ArucoPipeline.AddProcessor(_arProcessor);

            _arOutput = new JsonOutput(OnPoseChanged);
            ArucoPipeline.AddOutput(_arOutput);
        }

        void OnDisable()
        {
            ArucoPipeline.RemoveProcessor(_arProcessor);
            ArucoPipeline.RemoveOutput(_arOutput);
        }

        private void OnPoseChanged(string json_msg)
        {
            _currentOutput = JsonUtility.FromJson<ArucoOutput>(json_msg);
            _hasNewOutput = true;
        }

        void Update()
        {
            if (_hasNewOutput)
            {
                _hasNewOutput = false;
                var output = _currentOutput;

                // TODO: processor only processes markers on left image at the moment!
                foreach (var marker in output.markers_left)
                {
                    var markerPose = ProcessOutput(marker);

                    if (DetectedPoses.ContainsKey(marker.id))
                        DetectedPoses[marker.id] = markerPose;
                    else
                        DetectedPoses.Add(marker.id, markerPose);
                    
                    if (NewPoseDetected != null)
                        NewPoseDetected(markerPose);
                }
            }
        }


        public bool UseOffset = true;

        private MarkerPose ProcessOutput(ArucoPose pose)
        {
            // TODO: should be HMDGap from OvrVision..?
            var posOffset = new Vector3(-0.032f, 0.0f, 0.0f);

            if (!UseOffset)
            {
                posOffset = Vector3.zero;
            }

            // Rotation offset to match Optitrack's Calibration Helper
            //var rotOffset = Quaternion.Euler(RotationOffset);
            var rotation = pose.rotation.ToUnityQuaternion().eulerAngles;

            return new MarkerPose
            {
                Id = pose.id,
                Position = pose.position.ToUnityVec3() + posOffset,
                //Rotation = pose.rotation.ToUnityQuaternion() * rotOffset
                Rotation = Quaternion.Euler(rotation)
            };
        }
    }
}
