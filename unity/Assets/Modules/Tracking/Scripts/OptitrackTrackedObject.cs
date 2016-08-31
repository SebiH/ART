using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Modules.Tracking
{
    /// <summary>
    /// Track object through SlipStream (NatNet's local server that streams via XML to Unity)
    /// </summary>
    public class OptitrackTrackedObject : MonoBehaviour
    {
        // Name in OptiTrack
        public string TrackedName = "";

        public bool TrackPosition = true;
        public bool TrackOrientation = true;

        public bool DrawMarkers;

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnPosesReceived;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnPosesReceived;
        }

        void OnPosesReceived(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName != TrackedName)
                {
                    continue;
                }

                if (TrackPosition)
                {
                    transform.position = pose.Position;
                }

                if (TrackOrientation)
                {
                    transform.rotation = pose.Rotation;
                }

                if (DrawMarkers)
                {
                    foreach (var marker in pose.Markers)
                    {
                        string markerName = String.Format("Marker{0}_{1}", pose.Id, marker.Id);

                        var markerObj = GameObject.Find(markerName);

                        if (markerObj == null)
                        {
                            markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);
                            markerObj.transform.parent = transform;
                            markerObj.transform.localScale = scale;
                            markerObj.name = markerName;
                        }

                        markerObj.transform.position = marker.Position;
                    }
                }
            }
        }
    }
}
