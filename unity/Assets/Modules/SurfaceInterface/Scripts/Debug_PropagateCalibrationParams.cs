using Assets.Modules.Calibration;
using Assets.Modules.Core;
using Assets.Modules.Surfaces;
using System;
using UnityEngine;

namespace Assets.Modules.SurfaceInterface
{
    [RequireComponent(typeof(DynamicCalibration))]
    public class Debug_PropagateCalibrationParams : MonoBehaviour
    {
        public ChangeMonitor Monitor;

        private void Update()
        {
            // TODO: dynamic calibration debug info?

            Monitor.UpdateStability(CalibrationParams.PositionOffset, CalibrationParams.RotationOffset);
            RemoteSurfaceConnection.Instance.SendCommand(Globals.DefaultSurfaceName, "debug-calibration", JsonUtility.ToJson(new Packet
            {
                posOffsetX = CalibrationParams.PositionOffset.x,
                posOffsetY = CalibrationParams.PositionOffset.y,
                posOffsetZ = CalibrationParams.PositionOffset.z,

                rotOffsetX = CalibrationParams.RotationOffset.x,
                rotOffsetY = CalibrationParams.RotationOffset.y,
                rotOffsetZ = CalibrationParams.RotationOffset.z,
                rotOffsetW = CalibrationParams.RotationOffset.w,

                stability = Monitor.Stability,
                posStability = Monitor.PositionStability,
                rotStability = Monitor.RotationStability
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

            public float stability;
            public float posStability;
            public float rotStability;

            public float lastUpdateTime;
        }
    }
}
