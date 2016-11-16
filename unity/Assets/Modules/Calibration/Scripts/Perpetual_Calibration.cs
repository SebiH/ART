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
        public float ArCutoffTime = 0.2f;
        public float OptitrackCutoffTime = 0.2f;

        public float NearestDistanceThreshold = 0.3f;
        public float MinMarkerAngle = 35f;
        public float MaxMarkerCameraDistance = 0.6f;

        public string DisplayName = "Surface";

        // Camera used to determine which marker should be selected for calibration
        public Transform TrackedCamera;

        public string OptitrackCameraName = "HMD";
        private OptitrackPose _optitrackPose;
        private float _optitrackPoseTime;

        private Quaternion _ovrRotation;

        private List<DisplayMarker> _markers = new List<DisplayMarker>();

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

        private List<Vector3> _perMarkerPosCalib = new List<Vector3>();
        private List<Quaternion> _perMarkerRotCalib = new List<Quaternion>();


        private bool _isInitialised = false;
        void Update()
        {
            if (!_isInitialised)
            {
                if (FixedDisplays.Has(DisplayName))
                {
                    _isInitialised = true;
                    // fetch markers once display is initialised
                    InteractiveSurfaceClient.Instance.SendCommand(new WebCommand
                    {
                        command = "get-marker",
                        payload = null,
                        target = DisplayName
                    });
                }
            }

            if (FixedDisplays.Has(DisplayName) && Time.unscaledTime - _optitrackPoseTime < OptitrackCutoffTime)
            {
                var display = FixedDisplays.Get(DisplayName);

                // get nearest marker (from center) with up-to-date ar marker pose
                MarkerPose nearestMarker = null;
                var nearestDistance = 0f;

                // get intersection between camera ray and display plane
                Vector3 intersection = new Vector3();
                var hasIntersection = MathUtility.LinePlaneIntersection(out intersection, TrackedCamera.position, TrackedCamera.forward, display.Normal, display.GetCornerPosition(Corner.TopLeft));

                var angle = MathUtility.AngleVectorPlane(TrackedCamera.forward, display.Normal);
                Debug.Log(angle);

                if (hasIntersection)
                {
                    foreach (var marker in ArucoListener.Instance.DetectedPoses.Values)
                    {
                        bool isCurrent = (Time.unscaledTime - marker.DetectionTime) < ArCutoffTime;
                        if (!isCurrent) { continue; } // shave off a few calculations

                        // calculate distance between intersection and marker
                        var markerWorldPosition = GetMarkerWorldPosition(marker.Id);
                        var distance = (intersection - markerWorldPosition).sqrMagnitude;
                        bool isNearest = (nearestMarker == null) || ((distance < nearestDistance));

                        if (isCurrent && isNearest)
                        {
                            nearestMarker = marker;
                            nearestDistance = distance;
                        }
                    }
                }

                if (nearestMarker != null && nearestDistance < NearestDistanceThreshold)
                {
                    var markerWorldPos = GetMarkerWorldPosition(nearestMarker.Id);
                    var distMarkerToCamera = (markerWorldPos - TrackedCamera.position).sqrMagnitude;

                    if (distMarkerToCamera > MaxMarkerCameraDistance)
                    {
                        return;
                    }

                    if (__nearestMarkerId != nearestMarker.Id)
                    {
                        _perMarkerPosCalib.Clear();
                        _perMarkerRotCalib.Clear();
                    }


                    // debugging
                    __nearestMarkerId = nearestMarker.Id;

                    // apply calibration
                    var markerMatrix = Matrix4x4.TRS(nearestMarker.Position, nearestMarker.Rotation, Vector3.one);
                    var cameraMatrix = markerMatrix.inverse;

                    var localPos = cameraMatrix.GetPosition();
                    var worldPos = markerWorldPos + display.Rotation * localPos;

                    var localRot = cameraMatrix.GetRotation();
                    var localForward = localRot * Vector3.forward;
                    var localUp = localRot * Vector3.up;

                    var worldForward = display.Rotation * localForward;
                    var worldUp = display.Rotation * localUp;
                    var worldRot = Quaternion.LookRotation(worldForward, worldUp);

                    _perMarkerPosCalib.Add(Quaternion.Inverse(_optitrackPose.Rotation) * (worldPos - _optitrackPose.Position));
                    _perMarkerRotCalib.Add(worldRot * Quaternion.Inverse(_ovrRotation));

                    if (_perMarkerPosCalib.Count > 5 && _perMarkerPosCalib.Count < 50)
                    {
                        CalibrationParams.OptitrackToCameraOffset = MathUtility.Average(_perMarkerPosCalib);
                        // c = b * inv(a)
                        // => b = c * a?
                        // from ovrRot to worldRot
                        CalibrationParams.OpenVrRotationOffset = MathUtility.Average(_perMarkerRotCalib);
                    }
                }
            }
        }

        private Vector3 GetMarkerWorldPosition(int id)
        {
            var marker = _markers.FirstOrDefault((m) => m.id == id);
            var display = FixedDisplays.Get(DisplayName);

            if (marker == null)
            {
                // should never happen, in theory
                Debug.Log("Unable to find marker " + id);
                return Vector3.zero;
            }

            /*
             * Display:
             *       x
             *   +------->
             *   |
             *  y|
             *   v
             *
             * Unity:
             *  ^   /
             * y|  /z
             *  | /
             *  +--------
             *    x
             *
             *
             * (x,y) (display)
             * =
             * (x,-z) (unity) (display assumed to be horizontal in unity coords)
             */

            // posX/Y points to topleft corner of marker; we need center for calibration purposes
            var markerOffset = DisplayUtility.PixelToUnityCoord(marker.size) / 2f;

            // origin of marker coordinates is top-left corner;
            var unityPosX = DisplayUtility.PixelToUnityCoord(marker.posX) + markerOffset;
            var unityPosY = 0f; // marker lies directly on display
            var unityPosZ = -(DisplayUtility.PixelToUnityCoord(marker.posY) + markerOffset);

            var worldOffsetFromTopLeft = new Vector3(unityPosX, unityPosY, unityPosZ);
            var localOffsetFromTopLeft = display.Rotation * worldOffsetFromTopLeft;

            return display.GetCornerPosition(Corner.TopLeft) + localOffsetFromTopLeft;
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
                var payload = JsonUtility.FromJson<DisplayMarker>(cmd.payload);
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

                var unitySize = DisplayUtility.PixelToUnityCoord(payload.size);
                ArucoListener.Instance.MarkerSizeInMeter = unitySize;
            }
        }


        #region Debugging

        private int __nearestMarkerId = -1;

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            if (!FixedDisplays.Has(DisplayName))
                return;

            var display = FixedDisplays.Get(DisplayName);
            var displayRotation = display.Rotation;

            foreach (var marker in _markers)
            {
                if (marker.id == __nearestMarkerId)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.blue;

                var markerPos = GetMarkerWorldPosition(marker.id);

                Gizmos.DrawWireSphere(markerPos, 0.01f);

                var offset = DisplayUtility.PixelToUnityCoord(marker.size) / 2;
                var topleft = markerPos - displayRotation * new Vector3(-offset, 0, offset);
                var bottomleft = markerPos - displayRotation * new Vector3(-offset, 0, -offset);
                var bottomright = markerPos - displayRotation * new Vector3(offset, 0, -offset);
                var topright = markerPos - displayRotation * new Vector3(offset, 0, offset);

                Gizmos.DrawLine(topleft, bottomleft);
                Gizmos.DrawLine(bottomleft, bottomright);
                Gizmos.DrawLine(bottomright, topright);
                Gizmos.DrawLine(topright, topleft);
            }
        }

        #endregion
    }
}
