using Assets.Modules.Core;
using Assets.Modules.Tracking;
using Assets.Modules.Tracking.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.InteractiveSurface
{
    public class TableSetup_Aruco : MonoBehaviour
    {
        public int TopLeftId = 682;
        public int BottomLeftId = 680;
        public int TopRightId = 683;
        public int BottomRightId = 681;

        // Border between edge of Aruco Marker and display end
        public float BorderWidthCm = 0f;
        public int MinSampleSize = 100;

        public GameObject InteractiveSurfaceTemplate;

        private class AveragePose
        {
            public int SampleSize { get; private set; }
            private List<Vector3> _positions = new List<Vector3>();
            private List<Quaternion> _rotations = new List<Quaternion>();

            public void AddSample(Vector3 pos, Quaternion rot)
            {
                _positions.Add(pos);
                _rotations.Add(rot);
                SampleSize++;
            }

            public Vector3 GetAveragePosition()
            {
                Vector3 avgPos = Vector3.zero;
                foreach (var pos in _positions)
                {
                    avgPos += pos;
                }
                return avgPos / SampleSize;
            }

            public Quaternion GetAverageRotation()
            {
                return MathUtility.Average(_rotations);
            }
        }

        private const int TOPLEFT = 0;
        private const int BOTTOMLEFT = 1;
        private const int BOTTOMRIGHT = 2;
        private const int TOPRIGHT = 3;
        private AveragePose[] _poses = new AveragePose[4];


        void OnEnable()
        {
            ArMarkerTracker.Instance.NewPoseDetected += OnArucoPose;
            for (int i = 0; i < _poses.Length; i++)
            {
                _poses[i] = new AveragePose();
            }
        }

        void OnDisable()
        {
            ArMarkerTracker.Instance.NewPoseDetected -= OnArucoPose;
        }


        private void OnArucoPose(MarkerPose pose)
        {
            var sceneCamTransform = SceneCameraTracker.Instance.transform;
            int index = -1;

            if (pose.Id == TopLeftId)
                index = TOPLEFT;
            else if (pose.Id == BottomRightId)
                index = BOTTOMRIGHT;
            else if (pose.Id == TopRightId)
                index = TOPRIGHT;
            else if (pose.Id == BottomLeftId)
                index = BOTTOMLEFT;
            else
                return;

            var worldPos = sceneCamTransform.TransformPoint(pose.Position);
            var worldRot = sceneCamTransform.rotation * pose.Rotation;

            _poses[index].AddSample(worldPos, worldRot);
        }

        private void Update()
        {
            // TODO: show preview of currently calibrated surface
            foreach (var avgPose in _poses)
            {
                if (avgPose.SampleSize < MinSampleSize)
                {
                    // not yet ready
                    return;
                }
            }

            // all poses have reached enough samplesizes
            var centerPos = (_poses[TOPLEFT].GetAveragePosition() + _poses[TOPRIGHT].GetAveragePosition() +
                _poses[BOTTOMLEFT].GetAveragePosition() + _poses[BOTTOMRIGHT].GetAveragePosition()) / 4;

            var rightVector = _poses[TOPRIGHT].GetAveragePosition() - _poses[TOPLEFT].GetAveragePosition();
            var forwardVector =  _poses[TOPLEFT].GetAveragePosition() - _poses[BOTTOMLEFT].GetAveragePosition();
            var upVector = Vector3.Cross(forwardVector, rightVector);
            var avgRotation = Quaternion.LookRotation(forwardVector, upVector);

            //var avgMarkerRotation = QuaternionUtils.Average(new[]
            //{
            //    _poses[TOPLEFT].GetAverageRotation(),
            //    _poses[TOPRIGHT].GetAverageRotation(),
            //    _poses[BOTTOMLEFT].GetAverageRotation(),
            //    _poses[BOTTOMRIGHT].GetAverageRotation()
            //});

            var diagonal = _poses[BOTTOMRIGHT].GetAveragePosition() - _poses[TOPLEFT].GetAveragePosition();
            // invert rotation on diagonal so that forward == 0,0,1
            diagonal = Quaternion.Inverse(avgRotation) * diagonal;

            var markerSize = ArMarkerTracker.Instance.MarkerSizeInMeter;
            // add borderWidth + half marker size (since position is from marker center) once for both sides (*2)
            var scale = new Vector3(Mathf.Abs(diagonal.x) + 2 * BorderWidthCm / 100 + markerSize, 0.05f, Mathf.Abs(diagonal.z) + 2 * BorderWidthCm / 100 + markerSize);

            var surface = Instantiate(InteractiveSurfaceTemplate);
            surface.transform.position = centerPos;
            surface.transform.rotation = avgRotation;
            surface.transform.localScale = scale;

            // this script has served its purpose
            enabled = false;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                bool hasSamplesInAllEdges = true;
                foreach (var avgPose in _poses)
                {
                    if (avgPose.SampleSize > 0)
                    {
                        if (avgPose.SampleSize >= MinSampleSize)
                        {
                            Gizmos.color = Color.blue;
                        }
                        else
                        {
                            Gizmos.color = Color.Lerp(Color.red, Color.green, avgPose.SampleSize / (float)MinSampleSize);
                        }

                        var avgPosition = avgPose.GetAveragePosition();
                        Gizmos.DrawSphere(avgPosition, 0.03f);

                        var avgRotation = avgPose.GetAverageRotation();
                        var up = avgRotation * Vector3.up * 0.1f;
                        var forward = avgRotation * Vector3.forward * 0.1f;
                        var right = avgRotation * Vector3.right * 0.1f;
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(avgPosition, avgPosition + up);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(avgPosition, avgPosition + forward);
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(avgPosition, avgPosition + right);
                    }
                    else
                    {
                        hasSamplesInAllEdges = false;
                    }
                }


                if (hasSamplesInAllEdges)
                {
                    for (int i = 0; i < _poses.Length; i++)
                    {
                        Gizmos.DrawLine(_poses[i % _poses.Length].GetAveragePosition(), _poses[(i + 1) % _poses.Length].GetAveragePosition());
                    }


                    var centerPos = (_poses[TOPLEFT].GetAveragePosition() + _poses[TOPRIGHT].GetAveragePosition() +
                        _poses[BOTTOMLEFT].GetAveragePosition() + _poses[BOTTOMRIGHT].GetAveragePosition()) / 4;

                    //var avgRotation = QuaternionUtils.Average(new[]
                    //{
                    //    _poses[TOPLEFT].GetAverageRotation(),
                    //    _poses[TOPRIGHT].GetAverageRotation(),
                    //    _poses[BOTTOMLEFT].GetAverageRotation(),
                    //    _poses[BOTTOMRIGHT].GetAverageRotation()
                    //});

                    var rightVector = _poses[TOPRIGHT].GetAveragePosition() - _poses[TOPLEFT].GetAveragePosition();
                    var forwardVector =  _poses[TOPLEFT].GetAveragePosition() - _poses[BOTTOMLEFT].GetAveragePosition();
                    var upVector = Vector3.Cross(forwardVector, rightVector);
                    var avgRotation = Quaternion.LookRotation(forwardVector, upVector);

                    var up = avgRotation * Vector3.up * 0.1f;
                    var forward = avgRotation * Vector3.forward * 0.1f;
                    var right = avgRotation * Vector3.right * 0.1f;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(centerPos, centerPos + up);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(centerPos, centerPos + forward);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(centerPos, centerPos + right);
                }
            }
        }
    }
}
