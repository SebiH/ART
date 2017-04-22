using Assets.Modules.Vision;
using Assets.Modules.Vision.Outputs;
using Assets.Modules.Vision.Processors;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ArucoMarkerTracker : ArMarkerTracker
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

        // Cannot be changed once script is running
        public Pipeline ArucoPipeline;

        public bool UseTracker = false;
        private bool _prevUseTracker;

        [Range(1.0f, 5.0f)]
        public float TrackerErrorRatio = 4.0f;
        private float _prevTrackerErrorRatio;

        private ArucoProcessor _arProcessor;
        private JsonOutput _arOutput;

        private Queue _currentOutput;

        protected override void OnEnable()
        {
            base.OnEnable();
            _currentOutput = Queue.Synchronized(new Queue());

            _arProcessor = new ArucoProcessor(MarkerSizeInMeter)
            {
                UseTracker = UseTracker,
                TrackerErrorRatio = TrackerErrorRatio
            };

            ArucoPipeline.AddProcessor(_arProcessor);

            _arOutput = new JsonOutput(OnPoseChanged);
            ArucoPipeline.AddOutput(_arOutput);

            _prevUseTracker = UseTracker;
            _prevTrackerErrorRatio = TrackerErrorRatio;
        }

        void OnDisable()
        {
            ArucoPipeline.RemoveProcessor(_arProcessor);
            ArucoPipeline.RemoveOutput(_arOutput);
        }

        private void OnPoseChanged(string json_msg)
        {
            _currentOutput.Enqueue(JsonUtility.FromJson<ArucoOutput>(json_msg));
        }

        void Update()
        {
            while (_currentOutput.Count > 0)
            {
                var output = (ArucoOutput)_currentOutput.Dequeue();

                // TODO: processor only processes markers on left image at the moment!
                foreach (var marker in output.markers_left)
                {
                    var markerPose = ProcessOutput(marker);
                    OnNewPoseDetected(markerPose);
                }
            }

            // TODO: refactor
            bool hasPropertyChanged =
                (_prevUseTracker != UseTracker) ||
                (_prevTrackerErrorRatio != TrackerErrorRatio);

            if (hasPropertyChanged)
            {
                _arProcessor.UseTracker = UseTracker;
                _arProcessor.TrackerErrorRatio = TrackerErrorRatio;
                UpdateProcessorProperties();
            }
        }

        protected override void UpdateMarkerSize(float size)
        {
            _arProcessor.MarkerSizeInMeter = MarkerSizeInMeter;
            UpdateProcessorProperties();
        }

        private void UpdateProcessorProperties()
        {
            _arProcessor.UpdateProperties();

            _prevUseTracker = UseTracker;
            _prevTrackerErrorRatio = TrackerErrorRatio;
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
                Confidence = 1,
                Position = pose.position.ToUnityVec3() + posOffset,
                //Rotation = pose.rotation.ToUnityQuaternion() * rotOffset
                Rotation = Quaternion.Euler(rotation)
            };
        }

        public override float GetMinConfidence()
        {
            return 0;
        }
    }
}
