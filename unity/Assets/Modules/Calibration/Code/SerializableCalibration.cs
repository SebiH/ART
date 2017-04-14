using System;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    [Serializable]
    public struct SerializableCalibration
    {
        public Vector3 PositionOffset;
        public Quaternion RotationOffset;
    }
}
