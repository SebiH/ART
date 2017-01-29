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
        private const int SAMPLES = 30;

        public string MonitorName = "";
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
            while (_positions.Count > SAMPLES)
            {
                _positions.RemoveAt(0);
            }

            var avgPosition = MathUtility.Average(_positions);
            var maxPosDiff = 0f;
            foreach (var pos in _positions)
            {
                maxPosDiff = Mathf.Max((pos - avgPosition).sqrMagnitude, maxPosDiff);
            }
            PositionStability = Mathf.Clamp(1 - maxPosDiff * Sensitivity * 10, 0f, 1f);


            _rotations.Add(nextRotation);
            while (_rotations.Count > SAMPLES)
            {
                _rotations.RemoveAt(0);
            }

            var avgRotation = MathUtility.Average(_rotations);
            var maxRotDiff = 0f;
            foreach (var rot in _rotations)
            {
                maxRotDiff = Mathf.Max(Quaternion.Angle(rot, avgRotation), maxRotDiff);
            }
            RotationStability = Mathf.Clamp(1 - maxRotDiff * (Sensitivity / 1000f), 0f, 1f);


            Stability = RotationStability * PositionStability;
        }

    }
}
