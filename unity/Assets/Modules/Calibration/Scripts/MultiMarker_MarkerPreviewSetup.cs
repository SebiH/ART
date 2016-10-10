using UnityEngine;
using Assets.Modules.Tracking;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Modules.Calibration
{
    public class MultiMarker_MarkerPreviewSetup : MonoBehaviour
    {
        // Clockwise: Anchor->Span1->Span2
        public int AnchorId = -1;
        public int Span1Id = -1;
        public int Span2Id = -1;
        public int UpId = -1;

        public string OptitrackCalibratorName = "CalibrationHelper";
        public Transform OptitrackCalibratorObject;

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnOptitrackPoses;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPoses;
        }

        private void OnOptitrackPoses(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCalibratorName)
                {
                    CalibrateMarker(pose);
                    enabled = false;
                }
            }
        }

        private void CalibrateMarker(OptitrackPose pose)
        {
            var anchorMarker = pose.Markers.First((m) => m.Id == AnchorId);
            var span1Marker = pose.Markers.First((m) => m.Id == Span1Id);
            var span2Marker = pose.Markers.First((m) => m.Id == Span2Id);
            var upMarker = pose.Markers.First((m) => m.Id == UpId);

            var normalVector = Vector3.Normalize(Vector3.Cross(span1Marker.Position - anchorMarker.Position, span2Marker.Position - anchorMarker.Position));
            var rotationAxis = Vector3.Normalize(Vector3.Cross(normalVector, -OptitrackCalibratorObject.up));
            var rotationAngle = Vector3.Dot(normalVector, -OptitrackCalibratorObject.up);

            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);

            //var ground = OptitrackCalibratorObject;
            //var groundPlane = new Plane(ground.position, ground.position + ground.forward, ground.position + ground.right);

            //var localUpVector = OptitrackCalibratorObject.up;
            //// assume that point is *above* plain, no idea if that's important in unity's algorithm
            //float distanceAnchorPlane = 0f;
            //groundPlane.Raycast(new Ray(anchorMarker.Position, -localUpVector), out distanceAnchorPlane);

            //float distanceSpan1Plane = 0f;
            //groundPlane.Raycast(new Ray(span1Marker.Position, -localUpVector), out distanceSpan1Plane);

            //// bring span1marker position to the same height (based on groundplane) as anchorMarker


            //var angleA1 =


            var markerSetup = GetComponent<MultiMarker_MarkerSetup>();
            markerSetup.CalibratorToMarkerRotOffset = rotation.eulerAngles;

            var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;
            markerSetup.CalibratorToMarkerPosOffset = new Vector3(-markerSize/2f, 0, -markerSize/2f);
        }
    }
}
