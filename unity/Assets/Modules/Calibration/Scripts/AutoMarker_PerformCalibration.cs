using Assets.Modules.Tracking;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration
{
    public class AutoMarker_PerformCalibration : MonoBehaviour
    {
        const float SteadyPosThreshold = 0.2f;
        const float SteadyAngleThreshold = 2f;

        public int MaxCalibrationSamples = 100;
        //public float CalibrationProgress { get; private set; }

        public struct MarkerOffset
        {
            public enum Corner
            {
                TopLeft,
                TopRight,
                BottomLeft,
                BottomRight
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

            public bool HasOptitrackPose { get; set; }
            public Vector3 OptitrackMarkerPosition { get; set; }
            public Quaternion OptitrackMarkerRotation { get; set; }
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


        void OnDrawGizmos()
        {
            //if (IsReadyForCalibration)
            //{
            //    var avgMarkerPos = Vector3.zero;
            //    var arucoPosesCount = 0;
            //    var arucoRotations = new List<Quaternion>();

            //    foreach (var pose in _calibratedArucoPoses)
            //    {
            //        avgMarkerPos += pose.Value.Position;
            //        arucoRotations.Add(pose.Value.Rotation);
            //        arucoPosesCount++;
            //    }

            //    var avgRotation = QuaternionUtils.Average(arucoRotations);
            //    avgMarkerPos = avgMarkerPos / arucoPosesCount;
            //    var avgPosOffset = (avgMarkerPos - _optitrackCameraPose.Position);

            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawSphere(_optitrackCameraPose.Position + avgPosOffset, 0.01f);
            //    var start = _optitrackCameraPose.Position + avgPosOffset;
            //    var end = start + (avgRotation * Vector3.forward);
            //    var up = start + (avgRotation * Vector3.up * 0.1f);
            //    var right = start + (avgRotation * Vector3.right * 0.1f);
            //    Gizmos.DrawLine(start, end);
            //    Gizmos.DrawLine(start, up);
            //    Gizmos.DrawLine(start, right);

            //    Gizmos.color = Color.green;
            //    end = start + (_ovrRot * Vector3.forward);
            //    up = start + (_ovrRot * Vector3.up * 0.1f);
            //    right = start + (_ovrRot * Vector3.right * 0.1f);
            //    Gizmos.DrawLine(start, end);
            //    Gizmos.DrawLine(start, up);
            //    Gizmos.DrawLine(start, right);

            //    foreach (var pose in _calibratedArucoPoses)
            //    {
            //        Gizmos.color = Color.red;
            //        var rot = pose.Value.Rotation;
            //        start = pose.Value.Position;
            //        end = start + (rot * Vector3.forward);
            //        up = start + (rot * Vector3.up * 0.1f);
            //        right = start + (rot * Vector3.right * 0.1f);
            //        Gizmos.DrawLine(start, end);
            //        Gizmos.DrawLine(start, up);
            //        Gizmos.DrawLine(start, right);

            //        // draw ray from marker through possible camera location
            //        var marker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == pose.Key);
            //        start = marker.Marker.transform.position;
            //        var direction = pose.Value.Position - start;
            //        end = start + 2 * direction;
            //        Gizmos.color = Color.yellow;
            //        Gizmos.DrawLine(start, end);
            //    }

            //    var poses = _calibratedArucoPoses.Values.ToArray();
            //    Gizmos.color = Color.black;
            //    var closestPoints = new List<Vector3>();

            //    for (int poseIndex = 0; poseIndex < poses.Length; poseIndex++)
            //    {
            //        var pose = poses[poseIndex];
            //        var poseMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == pose.Id).Marker;
            //        var poseRay = new Ray(poseMarker.transform.position, pose.Position - poseMarker.transform.position);

            //        for (int intersectIndex = poseIndex + 1; intersectIndex < poses.Length; intersectIndex++)
            //        {
            //            var intersect = poses[intersectIndex];
            //            var intersectMarker = _markerSetupScript.CalibratedMarkers.First((m) => m.Id == intersect.Id).Marker;
            //            var intersectRay = new Ray(intersectMarker.transform.position, intersect.Position - intersectMarker.transform.position);

            //            Vector3 closestPoint1, closestPoint2;
            //            var success = Math3d.ClosestPointsOnTwoLines(out closestPoint1, out closestPoint2, poseRay.origin, poseRay.direction, intersectRay.origin, intersectRay.direction);

            //            if (success)
            //            {
            //                Gizmos.DrawSphere(closestPoint1, 0.005f);
            //                Gizmos.DrawSphere(closestPoint2, 0.005f);
            //                closestPoints.Add(closestPoint1);
            //                closestPoints.Add(closestPoint2);
            //            }
            //        }
            //    }

            //    if (closestPoints.Count > 0)
            //    {
            //        var avgIntersect = Vector3.zero;
            //        foreach (var p in closestPoints)
            //        {
            //            avgIntersect += p;
            //        }

            //        avgIntersect = avgIntersect / closestPoints.Count;
            //        Gizmos.color = Color.white;
            //        Gizmos.DrawSphere(avgIntersect, 0.01f);
            //    }
            //}
        }
    }
}
