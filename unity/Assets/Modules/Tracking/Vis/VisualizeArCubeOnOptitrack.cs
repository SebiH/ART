﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Modules.Tracking
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
                    markerPosInRoom += (matchingMarker.Position + calibOffset.Position);
                }

                markerPosInRoom = markerPosInRoom / CalibrationOffsets.Count;
            }

            transform.localPosition = markerPosInRoom;
            transform.localRotation = _optitrackCalibrationPose.Rotation;
            // rotate around 180 degrees because marker is on the backside
            //Quaternion offset = Quaternion.Euler(180f, 0f, 0f);
            //transform.localRotation *= offset;
        }
    }
}
