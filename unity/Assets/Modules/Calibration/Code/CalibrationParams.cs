using UnityEngine;

namespace Assets.Modules.Calibration
{
    public static class CalibrationParams
    {
        private static readonly int STABLE_SAMPLE_COUNT = 100;
        private static readonly float AVG_WEIGHT = 0.95f;

        public static void Reset()
        {
            ResetRotationOffset();
            ResetPositionOffset();
        }

        #region Rotation

        public static float LastCalibrationTime { get; private set; }

        private static Quaternion _rotationOffset = Quaternion.identity;
        public static Quaternion RotationOffset
        {
            get { return _rotationOffset; }
            set { CalculateOffset(value); }
        }

        public static bool HasStableRotation
        {
            get { return _avgRotSamples >= STABLE_SAMPLE_COUNT; }
        }

        public static void ResetRotationOffset()
        {
            _avgRotSamples = 0;
            _rotationOffset = Quaternion.identity;
        }

        private static int _avgRotSamples = 0;
        private static void CalculateOffset(Quaternion currentValue)
        {
            if (_avgRotSamples < STABLE_SAMPLE_COUNT)
            {
                // use normal moving average until we have enough samples
                float x = (_avgRotSamples * _rotationOffset.x + currentValue.x) / (_avgRotSamples + 1);
                float y = (_avgRotSamples * _rotationOffset.y + currentValue.y) / (_avgRotSamples + 1);
                float z = (_avgRotSamples * _rotationOffset.z + currentValue.z) / (_avgRotSamples + 1);
                float w = (_avgRotSamples * _rotationOffset.w + currentValue.w) / (_avgRotSamples + 1);

                _rotationOffset = new Quaternion(x, y, z, w);
                _avgRotSamples++;
            }
            else
            {
                // use decaying moving average to discard old values over time
                float x = (AVG_WEIGHT * _rotationOffset.x + (1 - AVG_WEIGHT) * currentValue.x);
                float y = (AVG_WEIGHT * _rotationOffset.y + (1 - AVG_WEIGHT) * currentValue.y);
                float z = (AVG_WEIGHT * _rotationOffset.z + (1 - AVG_WEIGHT) * currentValue.z);
                float w = (AVG_WEIGHT * _rotationOffset.w + (1 - AVG_WEIGHT) * currentValue.w);

                _rotationOffset = new Quaternion(x, y, z, w);
            }

            LastCalibrationTime = Time.unscaledTime;
        }

        #endregion


        #region Position

        private static Vector3 _positionOffset = Vector3.zero;
        public static Vector3 PositionOffset
        {
            get { return _positionOffset; }
            set { CalculateOffset(value); }
        }

        public static bool HasStablePosition
        {
            get { return _avgPosSamples >= STABLE_SAMPLE_COUNT; }
        }

        public static void ResetPositionOffset()
        {
            _avgPosSamples = 0;
            _positionOffset = Vector3.zero;
        }


        private static int _avgPosSamples = 0;
        private static void CalculateOffset(Vector3 currentValue)
        {
            if (_avgPosSamples < STABLE_SAMPLE_COUNT)
            {
                // use normal moving average until we have enough samples
                float x = (_avgPosSamples * _positionOffset.x + currentValue.x) / (_avgPosSamples + 1);
                float y = (_avgPosSamples * _positionOffset.y + currentValue.y) / (_avgPosSamples + 1);
                float z = (_avgPosSamples * _positionOffset.z + currentValue.z) / (_avgPosSamples + 1);

                _positionOffset = new Vector3(x, y, z);
                _avgPosSamples++;
            }
            else
            {
                // use decaying moving average to discard old values over time
                float x = (AVG_WEIGHT * _positionOffset.x + (1 - AVG_WEIGHT) * currentValue.x);
                float y = (AVG_WEIGHT * _positionOffset.y + (1 - AVG_WEIGHT) * currentValue.y);
                float z = (AVG_WEIGHT * _positionOffset.z + (1 - AVG_WEIGHT) * currentValue.z);

                _positionOffset = new Vector3(x, y, z);
            }

            LastCalibrationTime = Time.unscaledTime;
        }

        #endregion


        public static void PrimeCalibration(Vector3 positionOffset, Quaternion rotationOffset)
        {
            _avgPosSamples = STABLE_SAMPLE_COUNT;
            _positionOffset = positionOffset;

            _avgRotSamples = STABLE_SAMPLE_COUNT;
            _rotationOffset = rotationOffset;
        }
    }
}
