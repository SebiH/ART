using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using UnityEngine;

namespace Assets.Modules.Tracking.Scripts
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
        public double MarkerSizeInMeter = 0.15;

        public delegate void NewPoseHandler(ArucoMarkerPose pose);
        public event NewPoseHandler NewPoseDetected;

        private ArucoProcessor _arProcessor;
        private JsonOutput _arOutput;

        private bool _hasNewOutput = false;
        private ArucoOutput _currentOutput;

        void OnEnable()
        {
            Instance = this;

            _arProcessor = new ArucoProcessor(MarkerSizeInMeter);
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
            try
            {
                _currentOutput = JsonUtility.FromJson<ArucoOutput>(json_msg);
                _hasNewOutput = true;
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

                // TODO: processor only processes markers on left image at the moment!
                foreach (var marker in output.markers_left)
                {
                    var markerPose = ProcessOutput(marker);
                    
                    if (NewPoseDetected != null)
                    {
                        NewPoseDetected(markerPose);
                    }
                }
            }
        }


        private ArucoMarkerPose ProcessOutput(ArucoPose pose)
        {
            // TODO: should be HMDGap from OvrVision..?
            var offset = new Vector3(-0.032f, 0.0f, 0.0f);

            return new ArucoMarkerPose
            {
                Id = pose.id,
                Position = pose.position.ToUnityVec3() - offset,
                Rotation = pose.rotation.ToUnityQuaternion()
            };
        }
    }
}
