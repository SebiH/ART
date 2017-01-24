using Assets.Modules.Calibration;
using Assets.Modules.Core;
using Assets.Modules.InteractiveSurface;
using Assets.Modules.Surfaces;
using Assets.Modules.Tracking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Calibration_Deprecated
{
    public class Perpetual_Calibration : MonoBehaviour
    {
        public float ArCutoffTime = 0.2f;
        public float OptitrackCutoffTime = 0.2f;

        public float NearestDistanceThreshold = 0.3f;
        public float MinMarkerAngle = 35f;
        public float MaxMarkerCameraDistance = 0.6f;

        public string SurfaceName = "Surface";

        public bool UseMaps = false;
        public bool UseMultiMarker = false;

        // Camera used to determine which marker should be selected for calibration
        public Transform TrackedCamera;

        public string OptitrackCameraName = "HMD";
        private OptitrackPose _optitrackPose;
        private float _optitrackPoseTime;

        private Quaternion _ovrRotation;

        private List<DisplayMarker> _markers = new List<DisplayMarker>();
        private List<DisplayMap> _maps = new List<DisplayMap>();

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
            SteamVR_Events.NewPoses.Listen(OnSteamVrPose);
            InteractiveSurfaceClient.Instance.OnMessageReceived += HandleMarkerMessage;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
            SteamVR_Events.NewPoses.Remove(OnSteamVrPose);
            InteractiveSurfaceClient.Instance.OnMessageReceived -= HandleMarkerMessage;
        }

        private List<Vector3> _perMarkerPosCalib = new List<Vector3>();
        private List<Quaternion> _perMarkerRotCalib = new List<Quaternion>();


        private bool _isInitialised = false;
        void Update()
        {
            if (!_isInitialised)
            {
                if (SurfaceManager.Instance.Has(SurfaceName))
                {
                    _isInitialised = true;
                    var command = UseMaps ? "get-maps" : "get-marker";
                    // fetch markers once display is initialised
                    InteractiveSurfaceClient.Instance.SendCommand(new OutgoingCommand
                    {
                        command = command,
                        payload = null,
                        target = SurfaceName
                    });
                }
            }

            if (SurfaceManager.Instance.Has(SurfaceName) && Time.unscaledTime - _optitrackPoseTime < OptitrackCutoffTime)
            {
                var surface = SurfaceManager.Instance.Get(SurfaceName);

                if (UseMaps)
                {
                    DoMapCalibration(surface);
                }
                else
                {
                    if (UseMultiMarker)
                        DoMultiMarkerCalibration(surface);
                    else
                        DoSingleMarkerCalibration(surface);
                }
            }
        }

        private void DoSingleMarkerCalibration(Surface surface)
        {
            // get nearest marker (from center) with up-to-date ar marker pose
            MarkerPose nearestMarker = null;
            var nearestDistance = 0f;

            // get intersection between camera ray and display plane
            Vector3 intersection = new Vector3();
            var hasIntersection = MathUtility.LinePlaneIntersection(out intersection, TrackedCamera.position, TrackedCamera.forward, surface.Normal, surface.GetCornerPosition(Corner.TopLeft));

            var angle = MathUtility.AngleVectorPlane(TrackedCamera.forward, surface.Normal);

            // use <, as we want the camera to be perpendicular to the table ( | camera, _ table)
            __camTableAngle = Mathf.Abs(angle);
            if (Mathf.Abs(angle) < MinMarkerAngle)
            {
                __hasWrongAngle = true;
                return;
            }
            __hasWrongAngle = false;

            if (hasIntersection)
            {
                __intersection = intersection;
                var poses = UseMaps ? ArucoMapListener.Instance.DetectedPoses.Values : ArucoListener.Instance.DetectedPoses.Values;

                foreach (var marker in poses)
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

                __camMarkerDistance = distMarkerToCamera;

                if (distMarkerToCamera > MaxMarkerCameraDistance)
                {
                    __isTooFarAway = true;
                    return;
                }
                __isTooFarAway = false;

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
                var worldPos = markerWorldPos + surface.Rotation * localPos;

                var localRot = cameraMatrix.GetRotation();
                var localForward = localRot * Vector3.forward;
                var localUp = localRot * Vector3.up;

                var worldForward = surface.Rotation * localForward;
                var worldUp = surface.Rotation * localUp;
                var worldRot = Quaternion.LookRotation(worldForward, worldUp);

                _perMarkerPosCalib.Add(Quaternion.Inverse(_optitrackPose.Rotation) * (worldPos - _optitrackPose.Position));
                _perMarkerRotCalib.Add(worldRot * Quaternion.Inverse(_ovrRotation));

                if (_perMarkerPosCalib.Count > 5 && _perMarkerPosCalib.Count < 50)
                {
                    CalibrationParams.PositionOffset = MathUtility.Average(_perMarkerPosCalib);
                    // c = b * inv(a)
                    // => b = c * a?
                    // from ovrRot to worldRot
                    CalibrationParams.RotationOffset = MathUtility.Average(_perMarkerRotCalib);
                }
            }
        }

        private void DoMultiMarkerCalibration(Surface surface)
        {
            // get nearest marker (from center) with up-to-date ar marker pose
            var nearestMarkers = new List<MarkerPose>();

            // get intersection between camera ray and display plane
            Vector3 intersection = new Vector3();
            var hasIntersection = MathUtility.LinePlaneIntersection(out intersection, TrackedCamera.position, TrackedCamera.forward, surface.Normal, surface.GetCornerPosition(Corner.TopLeft));

            var angle = MathUtility.AngleVectorPlane(TrackedCamera.forward, surface.Normal);

            // use <, as we want the camera to be perpendicular to the table ( | camera, _ table)
            __camTableAngle = Mathf.Abs(angle);
            if (Mathf.Abs(angle) < MinMarkerAngle)
            {
                __hasWrongAngle = true;
                return;
            }
            __hasWrongAngle = false;

            if (hasIntersection)
            {
                __intersection = intersection;
                var poses = UseMaps ? ArucoMapListener.Instance.DetectedPoses.Values : ArucoListener.Instance.DetectedPoses.Values;

                foreach (var marker in poses)
                {
                    bool isCurrent = (Time.unscaledTime - marker.DetectionTime) < ArCutoffTime;
                    if (!isCurrent) { continue; } // shave off a few calculations

                    // calculate distance between intersection and marker
                    var markerWorldPosition = GetMarkerWorldPosition(marker.Id);
                    var distance = (intersection - markerWorldPosition).sqrMagnitude;
                    bool isNear = distance < NearestDistanceThreshold;

                    if (isCurrent && isNear)
                    {
                        nearestMarkers.Add(marker);
                    }
                }
            }

            __nearestMarkerIds.Clear();

            var pOffsets = new List<Vector3>();
            var rOffsets = new List<Quaternion>();

            foreach (var nearestMarker in nearestMarkers)
            {
                __nearestMarkerIds.Add(nearestMarker.Id);

                var markerWorldPos = GetMarkerWorldPosition(nearestMarker.Id);
                var distMarkerToCamera = (markerWorldPos - TrackedCamera.position).sqrMagnitude;

                __camMarkerDistance = distMarkerToCamera;

                // debugging
                __nearestMarkerId = nearestMarker.Id;

                // apply calibration
                var markerMatrix = Matrix4x4.TRS(nearestMarker.Position, nearestMarker.Rotation, Vector3.one);
                var cameraMatrix = markerMatrix.inverse;

                var localPos = cameraMatrix.GetPosition();
                var worldPos = markerWorldPos + surface.Rotation * localPos;

                var localRot = cameraMatrix.GetRotation();
                var localForward = localRot * Vector3.forward;
                var localUp = localRot * Vector3.up;

                var worldForward = surface.Rotation * localForward;
                var worldUp = surface.Rotation * localUp;
                var worldRot = Quaternion.LookRotation(worldForward, worldUp);

                pOffsets.Add(Quaternion.Inverse(_optitrackPose.Rotation) * (worldPos - _optitrackPose.Position));
                rOffsets.Add(worldRot * Quaternion.Inverse(_ovrRotation));
            }

            if (nearestMarkers.Count > 0)
            {
                CalibrationParams.PositionOffset = MathUtility.Average(pOffsets);
                // c = b * inv(a)
                // => b = c * a?
                // from ovrRot to worldRot
                CalibrationParams.RotationOffset = MathUtility.Average(rOffsets);
            }
        }

        private void DoMapCalibration(Surface surface)
        {
            // TODO.
        }

        private Vector3 GetMapWorldPosition(int id)
        {
            var map = _maps.FirstOrDefault((m) => m.id == id);
            var surface = SurfaceManager.Instance.Get(SurfaceName);

            if (map == null)
            {
                // should never happen, in theory
                Debug.Log("Unable to find map " + id);
                return Vector3.zero;
            }

            var worldOffsetFromTopLeft = map.GetUnityPosition(surface);
            var localOffsetFromTopLeft = surface.Rotation * worldOffsetFromTopLeft;

            return surface.GetCornerPosition(Corner.TopLeft) + localOffsetFromTopLeft;
        }

        private Vector3 GetMarkerWorldPosition(int id)
        {
            var marker = _markers.FirstOrDefault((m) => m.id == id);
            var surface = SurfaceManager.Instance.Get(SurfaceName);

            if (marker == null)
            {
                // should never happen, in theory
                Debug.Log("Unable to find marker " + id);
                return Vector3.zero;
            }

            var worldOffsetFromTopLeft = marker.GetUnityPosition(surface);
            var localOffsetFromTopLeft = surface.Rotation * worldOffsetFromTopLeft;

            return surface.GetCornerPosition(Corner.TopLeft) + localOffsetFromTopLeft;
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

        private void OnSteamVrPose(TrackedDevicePose_t[] poses)
        {
            var i = (int)OpenVR.k_unTrackedDeviceIndex_Hmd;

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
            if (cmd.command == "map" && UseMaps)
            {
                var payload = JsonUtility.FromJson<DisplayMap>(cmd.payload);
                var existingMap = _maps.FirstOrDefault((m) => m.id == payload.id);

                if (existingMap == null)
                {
                    _maps.Add(payload);
                    var registeredMaps = ArucoMapListener.Instance.GetMaps();
                    registeredMaps.Add(new Vision.Processors.ArucoMapProcessor.Map
                    {
                        id = payload.id,
                        marker_size_m = 0.025f,
                        path = payload.GetConfigurationPath()
                    });
                    ArucoMapListener.Instance.UpdateMaps();
                    Debug.Log("registered map " + payload.id);
                }
                else
                {
                    existingMap.posX = payload.posX;
                    existingMap.posY = payload.posY;
                    existingMap.sizeX = payload.sizeX;
                    existingMap.sizeY = payload.sizeY;
                }

                // TODO.
                //var unitySize = DisplayUtility.PixelToUnityCoord(payload.size);
                //ArucoMapListener.Instance.MarkerSizeInMeter = unitySize;
            }

            if (cmd.command == "marker-data")
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

                // TODO1
                //var unitySize = DisplayUtility.PixelToUnityCoord(payload.size);
                //ArucoListener.Instance.MarkerSizeInMeter = unitySize;
            }

            if (cmd.command == "remove-marker")
            {
                var payload = int.Parse(cmd.payload);
                _markers.RemoveAll((m) => m.id == payload);
            }
        }


        #region Debugging

        // public for external debug menu/display
        public int __nearestMarkerId = -1;
        public List<int> __nearestMarkerIds = new List<int>();
        public bool __hasWrongAngle = false;
        public bool __isTooFarAway = false;
        public float __camTableAngle = 0;
        public float __camMarkerDistance = 0;
        public Vector3 __intersection;

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            if (!SurfaceManager.Instance.Has(SurfaceName))
            {
                Debug.Log("No display");
                return;
            }

            var surface = SurfaceManager.Instance.Get(SurfaceName);
            var displayRotation = surface.Rotation;

            Gizmos.DrawWireCube(__intersection, Vector3.one * 0.01f);

            foreach (var marker in _markers)
            {
                if (marker.id == __nearestMarkerId || __nearestMarkerIds.Contains(marker.id))
                {
                    if (__hasWrongAngle || __isTooFarAway)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.blue;
                    if (!UseMaps && ArucoListener.Instance.DetectedPoses.ContainsKey(marker.id)
                        && Time.unscaledTime - ArucoListener.Instance.DetectedPoses[marker.id].DetectionTime < ArCutoffTime)
                    {
                        Gizmos.color = Color.yellow;
                    }
                }

                var markerPos = GetMarkerWorldPosition(marker.id);

                Gizmos.DrawWireSphere(markerPos, 0.01f);

                var offset = surface.PixelToUnityCoord(marker.size) / 2;
                var topleft = markerPos - displayRotation * new Vector3(-offset, 0, offset);
                var bottomleft = markerPos - displayRotation * new Vector3(-offset, 0, -offset);
                var bottomright = markerPos - displayRotation * new Vector3(offset, 0, -offset);
                var topright = markerPos - displayRotation * new Vector3(offset, 0, offset);

                Gizmos.DrawLine(topleft, bottomleft);
                Gizmos.DrawLine(bottomleft, bottomright);
                Gizmos.DrawLine(bottomright, topright);
                Gizmos.DrawLine(topright, topleft);

                bool isValidMarker = ArucoListener.Instance.DetectedPoses.ContainsKey(marker.id) && Time.unscaledTime - ArucoListener.Instance.DetectedPoses[marker.id].DetectionTime < ArCutoffTime;

                if (isValidMarker)
                {
                    var arMarker = ArucoListener.Instance.DetectedPoses[marker.id];

                    var markerMatrix = Matrix4x4.TRS(arMarker.Position, arMarker.Rotation, Vector3.one);
                    var cameraMatrix = markerMatrix.inverse;

                    var localPos = cameraMatrix.GetPosition();
                    var worldPos = markerPos + surface.Rotation * localPos;

                    Gizmos.DrawWireSphere(worldPos, 0.01f);
                    Gizmos.DrawLine(worldPos, worldPos + Quaternion.Inverse(_optitrackPose.Rotation) * (worldPos - _optitrackPose.Position));

                    var localRot = cameraMatrix.GetRotation();
                    var localForward = localRot * Vector3.forward;
                    var localUp = localRot * Vector3.up;

                    var worldForward = surface.Rotation * localForward;
                    var worldUp = surface.Rotation * localUp;
                    var worldRight = surface.Rotation * (localRot * Vector3.right);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(worldPos, worldPos + worldForward * 0.1f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(worldPos, worldPos + worldUp * 0.01f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(worldPos, worldPos + worldRight * 0.01f);
                }
            }
        }

        #endregion
    }
}
