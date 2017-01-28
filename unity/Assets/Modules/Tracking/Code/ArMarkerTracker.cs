using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public abstract class ArMarkerTracker : MonoBehaviour
    {
        public static ArMarkerTracker Instance { get; protected set; }

        public readonly Dictionary<int, MarkerPose> DetectedPoses = new Dictionary<int, MarkerPose>();

        public delegate void NewPoseHandler(MarkerPose pose);
        public event NewPoseHandler NewPoseDetected;

        protected float _markerSize = 0.04f;
        public float MarkerSizeInMeter
        {
            get { return _markerSize; }
            set
            {
                if (_markerSize != value)
                {
                    _markerSize = value;
                    UpdateMarkerSize(value);
                }
            }
        }

        protected abstract void UpdateMarkerSize(float size);

        protected virtual void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple ArMarkerListeners in one scene is not supported!");
            }

            Instance = this;
        }


        protected void OnNewPoseDetected(MarkerPose pose)
        {
            DetectedPoses[pose.Id] = pose;

            if (NewPoseDetected != null)
            {
                NewPoseDetected(pose);
            }
        }

    }
}
