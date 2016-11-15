using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    [Serializable]
    public class OptitrackPose
    {
        [Serializable]
        public class Marker
        {
            public int Id;
            public Vector3 Position;
        }

        public int Id;
        public string RigidbodyName;

        public Vector3 Position;
        public Quaternion Rotation;

        public List<Marker> Markers = new List<Marker>();

        public float DetectionTime;
    }
}
