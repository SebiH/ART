using Assets.Modules.Calibration;
using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using System;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface
{
    [RequireComponent(typeof(DynamicCalibration), typeof(ChangeMonitor))]
    public class Debug_PropagateCalibrationParams : MonoBehaviour
    {
        private ChangeMonitor _monitor;

        private void OnEnable()
        {
            _monitor = GetComponent<ChangeMonitor>();
        }

        private void Update()
        {
            // TODO: dynamic calibration debug info?

            _monitor.UpdateStability(CalibrationParams.PositionOffset, CalibrationParams.RotationOffset);
            RemoteSurfaceConnection.SendCommand(Globals.DefaultSurfaceName, "debug-calibration", JsonUtility.ToJson(new Packet
            {
                posOffsetX = CalibrationParams.PositionOffset.x,
                posOffsetY = CalibrationParams.PositionOffset.y,
                posOffsetZ = CalibrationParams.PositionOffset.z,

                rotOffsetX = CalibrationParams.RotationOffset.x,
                rotOffsetY = CalibrationParams.RotationOffset.y,
                rotOffsetZ = CalibrationParams.RotationOffset.z,
                rotOffsetW = CalibrationParams.RotationOffset.w,

                lastUpdateTime = CalibrationParams.LastCalibrationTime
            }));
        }

        [Serializable]
        private struct Packet
        {
            public float posOffsetX;
            public float posOffsetY;
            public float posOffsetZ;

            public float rotOffsetX;
            public float rotOffsetY;
            public float rotOffsetZ;
            public float rotOffsetW;

            public float lastUpdateTime;
        }
    }
}
