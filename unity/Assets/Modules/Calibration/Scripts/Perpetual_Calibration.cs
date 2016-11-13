using Assets.Modules.Core;
using Assets.Modules.InteractiveSurface;
using Assets.Modules.Tracking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration
{
    public class Perpetual_Calibration : MonoBehaviour
    {
        public float ArCutoffTime = 1 / 30f;
        public float OptitrackCutoffTime = 1 / 30f;

        public string DisplayName = "Surface";

        // Camera used to determine which marker should be selected for calibration
        public Transform TrackedCamera;

        public string OptitrackCameraName = "HMD";
        private OptitrackPose _optitrackPose;
        private float _optitrackPoseTime;

        private Quaternion _ovrRotation;

        private List<Marker> _markers = new List<Marker>();

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Utils.Event.Listen("new_poses", OnSteamVrPose);
            InteractiveSurfaceClient.Instance.OnMessageReceived += HandleMarkerMessage;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Utils.Event.Remove("new_poses", OnSteamVrPose);
            InteractiveSurfaceClient.Instance.OnMessageReceived -= HandleMarkerMessage;
        }


        void Update()
        {
            if (FixedDisplays.Has(DisplayName) && Time.unscaledTime - _optitrackPoseTime < OptitrackCutoffTime)
            {
                var display = FixedDisplays.Get(DisplayName);

                // get nearest marker (from center) with up-to-date ar marker pose
                MarkerPose nearestMarker = null;
                var nearestDistance = 0f;

                // get intersection between camera ray and display plane
                Vector3 intersection = new Vector3();
                var hasIntersection = MathUtility.LinePlaneIntersection(out intersection, TrackedCamera.position, TrackedCamera.forward, display.Normal, display.GetCornerPosition(Corner.TopLeft));
                if (hasIntersection)
                {
                    foreach (var marker in ArucoListener.Instance.DetectedPoses.Values)
                    {
                        bool isCurrent = (Time.unscaledTime - marker.DetectionTime) < ArCutoffTime;
                        if (!isCurrent) { continue; } // shave off a few calculations

                        // calculate distance between intersection and marker
                        var markerWorldPosition = GetMarkerWorldPosition(marker.Id);
                        var distance = (intersection - markerWorldPosition).sqrMagnitude;
                        bool isNearest = (nearestMarker == null) || (distance < nearestDistance);

                        if (isCurrent && isNearest)
                        {
                            nearestMarker = marker;
                        }
                    }
                }

                if (nearestMarker != null)
                {
                    // apply calibration
                    var markerMatrix = Matrix4x4.TRS(nearestMarker.Position, nearestMarker.Rotation, Vector3.one);
                    var cameraMatrix = markerMatrix.inverse;

                    var localPos = cameraMatrix.GetPosition();
                    var worldPos = GetMarkerWorldPosition(nearestMarker.Id) + display.Rotation * localPos;

                    var localRot = cameraMatrix.GetRotation();
                    var localForward = localRot * Vector3.forward;
                    var localUp = localRot * Vector3.up;

                    var worldForward = display.Rotation * localForward;
                    var worldUp = display.Rotation * localUp;
                    var worldRot = Quaternion.LookRotation(worldForward, worldUp);

                    CalibrationParams.OptitrackToCameraOffset = Quaternion.Inverse(_optitrackPose.Rotation) * (worldPos - _optitrackPose.Position);
                    // c = b * inv(a)
                    // => b = c * a?
                    // from ovrRot to worldRot
                    CalibrationParams.OpenVrRotationOffset = worldRot * Quaternion.Inverse(_ovrRotation);
                }
            }
        }

        private Vector3 GetMarkerWorldPosition(int id)
        {
            // TODO.
            return Vector3.zero;
        }

        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCameraName)
                {
                    _optitrackPose = pose;
                    _optitrackPoseTime = Time.unscaledTime;
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

            _ovrRotation = pose.rot;
        }


        private void HandleMarkerMessage(IncomingCommand cmd)
        {
            if (cmd.command == "marker")
            {
                var payload = JsonUtility.FromJson<Marker>(cmd.payload);
                var existingMarker = _markers.FirstOrDefault((m) => m.id == payload.id);

                if (existingMarker == null)
                {
                    _markers.Add(payload);
                }
                else
                {
                    existingMarker.posX = payload.posX;
                    existingMarker.posY = payload.posY;
                    existingMarker.size = payload.size;
                }
            }
        }
    }
}
