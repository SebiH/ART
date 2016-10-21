using Assets.Modules.Tracking;
using Assets.Modules.Tracking.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration
{
    public class DisplayMarker_PerformCalibration : MonoBehaviour
    {
        const float SteadyPosThreshold = 0.2f;
        const float SteadyAngleThreshold = 2f;

        public float CalibrationStability { get; private set; }

        [Serializable]
        public struct MarkerOffset
        {
            public enum Corner
            {
                TopLeft = 0,
                TopRight = 1,
                BottomRight = 2,
                BottomLeft = 3
            }

            // set in editor
            public int ArMarkerId;
            public int OptitrackMarkerId;
            public Vector3 OtToArOffset;
            public Corner OptitrackCorner;

            // public getters & setters so that they don't show up in unity's editor
            public bool HasArPose { get; set; }
            public Vector3 ArMarkerPosition { get; set; }
            public Quaternion ArMarkerRotation { get; set; }
            public Vector3 ArCameraPosition { get; set; }
            public Quaternion ArCameraRotation { get; set; }

            public Vector3 OptitrackMarkerPosition { get; set; }
        }

        public List<MarkerOffset> CalibrationOffsets = new List<MarkerOffset>();
        public string OptitrackCameraName = "HMD";
        public string OptitrackDisplayName = "Display";
        private OptitrackPose _optitrackCameraPose;
        private OptitrackPose _optitrackDisplayPose;

        private Quaternion _ovrRot = Quaternion.identity;


        void OnEnable()
        {
            ArucoListener.Instance.NewPoseDetected += OnArucoPose;
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Utils.Event.Listen("new_poses", OnSteamVrPose);
        }

        void OnDisable()
        {
            ArucoListener.Instance.NewPoseDetected -= OnArucoPose;
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Utils.Event.Remove("new_poses", OnSteamVrPose);
        }


        void Update()
        {
            UpdateCalibration();
        }

        private void OnArucoPose(ArucoMarkerPose pose)
        {
            for (int i = 0; i < CalibrationOffsets.Count; i++)
            {
                var markerOffset = CalibrationOffsets[i];
                if (markerOffset.ArMarkerId == pose.Id)
                {
                    // pose is marker's pose -> inverted we get camera pose
                    var markerMatrix = Matrix4x4.TRS(pose.Position, pose.Rotation, Vector3.one);
                    var cameraMatrix = markerMatrix.inverse;
                    var cameraLocalPos = cameraMatrix.GetPosition();
                    var cameraLocalRot = cameraMatrix.GetRotation();

                    markerOffset.HasArPose = true;
                    markerOffset.ArMarkerPosition = pose.Position;
                    markerOffset.ArMarkerRotation = pose.Rotation;
                    markerOffset.ArCameraPosition = cameraMatrix.GetPosition();
                    markerOffset.ArCameraRotation = cameraMatrix.GetRotation();

                    break;
                }
            }
        }



        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCameraName)
                {
                    _optitrackCameraPose = pose;
                }

                if (pose.RigidbodyName == OptitrackDisplayName)
                {
                    _optitrackDisplayPose = pose;
                }
            }
        }


        private void OnSteamVrPose(params object[] args)
        {
            var i = (int)OpenVR.k_unTrackedDeviceIndex_Hmd;

            var poses = (TrackedDevicePose_t[])args[0];
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            _ovrRot = pose.rot;
        }


        private void UpdateCalibration()
        {
            
        }


        //public void Calibrate()
        //{
        //    StartCoroutine(DoCalibration());
        //}

        //private IEnumerator DoCalibration()
        //{
        //    var avgPosOffset = Vector3.zero;
        //    var arucoRotations = new List<Quaternion>();

        //    var samples = 0;
        //    IsCalibrating = true;

        //    while (samples < MaxCalibrationSamples)
        //    {
        //        foreach (var pose in _calibratedArucoPoses)
        //        {
        //            arucoRotations.Add(pose.Value.Rotation);
        //        }

        //        if (CalibMethod == CalibrationMethod.Standard)
        //        {
        //            var arucoPosesCount = 0;
        //            var avgMarkerPos = Vector3.zero;

        //            foreach (var pose in _calibratedArucoPoses)
        //            {
        //                avgMarkerPos += pose.Value.Position;
        //                arucoPosesCount++;
        //            }

        //            avgMarkerPos = avgMarkerPos / arucoPosesCount;
        //            avgPosOffset += (avgMarkerPos - _optitrackCameraPose.Position);
        //        }
        //        else if (CalibMethod == CalibrationMethod.LineExtension)
        //        {
        //            var poses = _calibratedArucoPoses.Values.ToArray();
        //            var closestPoints = new List<Vector3>();

        //            for (int poseIndex = 0; poseIndex < poses.Length; poseIndex++)
        //            {
        //                var pose = poses[poseIndex];
        //                var poseMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == pose.Id).Marker;
        //                var poseRay = new Ray(poseMarker.transform.position, pose.Position - poseMarker.transform.position);

        //                for (int intersectIndex = poseIndex + 1; intersectIndex < poses.Length; intersectIndex++)
        //                {
        //                    var intersect = poses[intersectIndex];
        //                    var intersectMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == intersect.Id).Marker;
        //                    var intersectRay = new Ray(intersectMarker.transform.position, intersect.Position - intersectMarker.transform.position);

        //                    Vector3 closestPoint1, closestPoint2;
        //                    var success = Math3d.ClosestPointsOnTwoLines(out closestPoint1, out closestPoint2, poseRay.origin, poseRay.direction, intersectRay.origin, intersectRay.direction);

        //                    if (success)
        //                    {
        //                        closestPoints.Add(closestPoint1);
        //                        closestPoints.Add(closestPoint2);
        //                    }
        //                }
        //            }

        //            if (closestPoints.Count > 0)
        //            {
        //                var avgIntersect = Vector3.zero;
        //                foreach (var p in closestPoints)
        //                {
        //                    avgIntersect += p;
        //                }

        //                avgIntersect = avgIntersect / closestPoints.Count;

        //                avgPosOffset += (avgIntersect - _optitrackCameraPose.Position);
        //            }
        //        }

        //        samples++;
        //        CalibrationProgress = samples / MaxCalibrationSamples;
        //        yield return new WaitForSeconds(0.1f);
        //    }

        //    avgPosOffset = avgPosOffset / samples;
        //    var avgRotation = QuaternionUtils.Average(arucoRotations);
        //    var avgRotOffset = avgRotation * Quaternion.Inverse(_ovrRot);

        //    CalibrationOffset.OptitrackToCameraOffset = avgPosOffset;
        //    CalibrationOffset.OpenVrRotationOffset = avgRotOffset;
        //    CalibrationOffset.IsCalibrated = true;
        //    CalibrationOffset.LastCalibration = DateTime.Now;

        //    Debug.Log("Calibration complete");
        //}

        private Quaternion CalculateTableRotation()
        {
            var markerBottomLeft = CalibrationOffsets.First((m) => m.OptitrackCorner == MarkerOffset.Corner.BottomLeft);
            var markerTopLeft = CalibrationOffsets.First((m) => m.OptitrackCorner == MarkerOffset.Corner.TopLeft);
            var markerBottomRight = CalibrationOffsets.First((m) => m.OptitrackCorner == MarkerOffset.Corner.BottomRight);

            var forward = Vector3.Normalize(markerTopLeft.OptitrackMarkerPosition - markerBottomLeft.OptitrackMarkerPosition);
            var right = Vector3.Normalize(markerBottomRight.OptitrackMarkerPosition - markerBottomLeft.OptitrackMarkerPosition);
            var up = Vector3.Cross(right, forward);

            return Quaternion.LookRotation(forward, up);
        }

        private Vector3 GetMarkerWorldPosition(MarkerOffset marker, Quaternion tableRotation)
        {
            // start at optitrack marker, add offset to marker's corner, then
            // add markersize to get center - in table's coordinate system (tableRotation)
            var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;
            var cornerOffset = Vector3.zero;

            switch (marker.OptitrackCorner)
            {
                case MarkerOffset.Corner.BottomLeft:
                    cornerOffset = new Vector3(markerSize / 2, 0, markerSize / 2);
                    break;

                case MarkerOffset.Corner.BottomRight:
                    cornerOffset = new Vector3(-markerSize / 2, 0, markerSize / 2);
                    break;

                case MarkerOffset.Corner.TopLeft:
                    cornerOffset = new Vector3(markerSize / 2, 0, -markerSize / 2);
                    break;

                case MarkerOffset.Corner.TopRight:
                    cornerOffset = new Vector3(-markerSize / 2, 0, -markerSize / 2);
                    break;
            }


            var start = marker.OptitrackMarkerPosition;
            return start + tableRotation * (marker.OtToArOffset + cornerOffset);
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // Draw OpenVR rotation
            {
                Gizmos.color = Color.green;
                var start = SceneCameraTracker.Instance.transform.position;
                var end = start + (_ovrRot * Vector3.forward);
                var up = start + (_ovrRot * Vector3.up * 0.1f);
                var right = start + (_ovrRot * Vector3.right * 0.1f);
                Gizmos.DrawLine(start, end);
                Gizmos.DrawLine(start, up);
                Gizmos.DrawLine(start, right);
            }

            // this only works if we have optitrack coordinates for all markers
            if (_optitrackDisplayPose != null)
            {
                var tableRotation = CalculateTableRotation();

                // draw table's orientation in center of table
                {
                    Vector3 tableCenter = Vector3.zero;
                    foreach (var marker in CalibrationOffsets)
                    {
                        tableCenter += marker.OptitrackMarkerPosition;
                    }
                    tableCenter /= CalibrationOffsets.Count;

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.up * 0.1f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.forward * 0.1f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(tableCenter, tableCenter + tableRotation * Vector3.right * 0.1f);
                }

                foreach (var marker in CalibrationOffsets)
                {
                    var nextMarkerCorner = ((MarkerOffset.Corner)(((int)marker.OptitrackCorner + 1) % Enum.GetNames(typeof(MarkerOffset.Corner)).Length));
                    var nextMarker = CalibrationOffsets.Find((m) => m.OptitrackCorner == nextMarkerCorner);

                    // draw lines around optitrack plane
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(marker.OptitrackMarkerPosition, nextMarker.OptitrackMarkerPosition);

                    // draw virtual position of calibrated markers, based on optitrack + measurements
                    Gizmos.color = Color.cyan;
                    var markerPosWorld = GetMarkerWorldPosition(marker, tableRotation);
                    var markerSize = (float)ArucoListener.Instance.MarkerSizeInMeter;
                    Gizmos.DrawWireSphere(markerPosWorld, 0.01f);
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, markerSize / 2), markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, markerSize / 2));
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, markerSize / 2), markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, -markerSize / 2));
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(-markerSize / 2, 0, -markerSize / 2), markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, -markerSize / 2));
                    Gizmos.DrawLine(markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, -markerSize / 2), markerPosWorld + tableRotation * new Vector3(markerSize / 2, 0, markerSize / 2));


                    // if available, draw world position of camera based on marker
                    if (marker.HasArPose)
                    {

                    }
                }
            }
        }
    }
}
