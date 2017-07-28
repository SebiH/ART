using Assets.Modules.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ChangeMonitor : MonoBehaviour
    {
        /// <summary> 0: unstable - 1: stable </summary>
        public float Stability { get; private set; }
        public float PositionStability { get; private set; }
        public float RotationStability { get; private set; }

        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<Quaternion> _rotations = new List<Quaternion>();

        public int MaxSamples = 100;
        public float Sensitivity = 20f;

        public void Reset()
        {
            Stability = 0;
            PositionStability = 0;
            RotationStability = 0;
            _positions.Clear();
            _rotations.Clear();
        }

        public void UpdateStability(Vector3 nextPosition, Quaternion nextRotation)
        {
            _positions.Add(nextPosition);
            while (_positions.Count > MaxSamples)
            {
                _positions.RemoveAt(0);
            }

            var avgPos = MathUtility.Average(_positions);
            var maxPosDifference = 0f;
            foreach (var pos in _positions)
            {
                maxPosDifference = Mathf.Max((pos - avgPos).magnitude, maxPosDifference);
            }
            PositionStability = Mathf.Clamp(1 - maxPosDifference * Sensitivity, 0f, 1f);


            _rotations.Add(nextRotation);
            while (_rotations.Count > MaxSamples)
            {
                _rotations.RemoveAt(0);
            }

            var avgRot = MathUtility.Average(_rotations);
            var maxRotDifference = 0f;
            foreach (var rot in _rotations)
            {
                maxRotDifference = Mathf.Max(Quaternion.Angle(rot, avgRot), maxRotDifference);
            }
            RotationStability = Mathf.Clamp(1 - maxRotDifference * (Sensitivity / 1000f), 0f, 1f);


            Stability = RotationStability * PositionStability;
        }

    }
}
