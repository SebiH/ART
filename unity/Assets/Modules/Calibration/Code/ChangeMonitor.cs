using Assets.Modules.Core;
using Assets.Modules.InteractiveSurface;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Calibration
{
    public class ChangeMonitor
    {
        /// <summary> 0: unstable - 1: stable </summary>
        public float Stability { get; private set; }

        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<Quaternion> _rotations = new List<Quaternion>();
        private const int SAMPLES = 30;

        private string _name;
        public float _sensitivity;

        public ChangeMonitor(string name, float sensitivity)
        {
            _name = name;
            _sensitivity = sensitivity;
        }

        public void Reset()
        {
            Stability = 0;
            _positions.Clear();
            _rotations.Clear();
        }

        public void Update(Vector3 nextPosition, Quaternion nextRotation)
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
            var positionStability = Mathf.Clamp(1 - maxPosDiff * _sensitivity, 0f, 1f);


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
            var rotationStability = Mathf.Clamp(1 - maxRotDiff * (_sensitivity / 1000f), 0f, 1f);


            Stability = rotationStability * positionStability;

#if UNITY_EDITOR
            if (InteractiveSurfaceClient.Instance)
            {
                InteractiveSurfaceClient.Instance.SendCommand(new OutgoingCommand
                {
                    command = "debug-cm-val-" + _name,
                    payload = Stability.ToString(),
                    target = "Surface"
                });
            }
#endif
        }

    }
}
