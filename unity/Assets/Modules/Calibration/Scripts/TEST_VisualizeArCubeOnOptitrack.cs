using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Modules.Tracking;

namespace Assets.Modules.Calibration
{

    public class VisualizeArCubeOnOptitrack : MonoBehaviour
    {
        public List<OptitrackPose.Marker> CalibrationOffsets = new List<OptitrackPose.Marker>();
        public string OptitrackCalibrationName = "CalibrationHelper";

        private OptitrackPose _optitrackCalibrationPose;

        void OnEnable()
        {
            OptitrackListener.Instance.PosesReceived += OnOptitrackPose;
        }

        void OnDisable()
        {
            OptitrackListener.Instance.PosesReceived -= OnOptitrackPose;
        }


        private void OnOptitrackPose(List<OptitrackPose> poses)
        {
            foreach (var pose in poses)
            {
                if (pose.RigidbodyName == OptitrackCalibrationName)
                {
                    _optitrackCalibrationPose = pose;
                    break;
                }
            }
        }


        void Update()
        {
            var markerPosInRoom = Vector3.zero;

            if (CalibrationOffsets.Count != 0 && _optitrackCalibrationPose != null)
            {
                // build average over all given offsets
                foreach (var calibOffset in CalibrationOffsets)
                {
                    var matchingMarker = _optitrackCalibrationPose.Markers.First(m => m.Id == calibOffset.Id);
                    markerPosInRoom += (matchingMarker.Position + _optitrackCalibrationPose.Rotation * calibOffset.Position);
                }

                markerPosInRoom = markerPosInRoom / CalibrationOffsets.Count;
            }

            transform.localPosition = markerPosInRoom;
            if (_optitrackCalibrationPose != null)
            {
                transform.localRotation = _optitrackCalibrationPose.Rotation;
            }
        }
    }
}
