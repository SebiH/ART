using UnityEngine;
using System.Collections;
using System;

namespace Assets.Modules.Tracking
{
    public class ApplyCameraCalibration : MonoBehaviour
    {
        void Update()
        {
            // TODO: calibration might be updated, maybe better to do this via event
            if (CalibrationOffset.IsCalibrated)
            {
                transform.localPosition = CalibrationOffset.OptitrackToCameraOffset;
                transform.localRotation = CalibrationOffset.OpenVrRotationOffset;

                enabled = false;
            }
        }
    }
}
