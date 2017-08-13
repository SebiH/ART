using Assets.Modules.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class PlaybackVrTracker : MonoBehaviour
    {
        public string Filename;
        [Range(-1, 1)]
        public float TimeOffset = 0;
        public int SmoothAverage = 3;

        public enum PlaybackTrackedDevice
        {
            LeftHand, RightHand, Head
        };
        public PlaybackTrackedDevice TrackedDevice = PlaybackTrackedDevice.LeftHand;

        [Serializable]
        private struct RecordedFile
        {
            public RecordUnityVr.RecordedPose[] entries;
        }
        private RecordedFile _cache;
        private int _currentIndex = 0;

        private void OnEnable()
        {
            var text = File.ReadAllText(FileUtility.GetPath(Filename));
            _cache = JsonUtility.FromJson<RecordedFile>(text);

            VRListener.UpdateRequested += Update;
        }

        private void Update()
        {
            while (_currentIndex < _cache.entries.Length - 1 && _cache.entries[_currentIndex + 1].Time <= PlaybackTime.RealTime + TimeOffset)
            {
                _currentIndex++;
            }

            if (_currentIndex < _cache.entries.Length)
            {
                VRListener.PoseUpdateTime = _cache.entries[_currentIndex].Time;

                List<Vector3> positions = new List<Vector3>();
                positions.Add(GetPosition(_currentIndex));
                for (var i = 1; i <= SmoothAverage; i++)
                {
                    if (_currentIndex + i < _cache.entries.Length)
                        positions.Add(GetPosition(_currentIndex + i));

                    if (_currentIndex - i >= 0)
                        positions.Add(GetPosition(_currentIndex - i));
                }
                VRListener.CurrentPosition = MathUtility.Average(positions);

                List<Quaternion> rotations = new List<Quaternion>();
                rotations.Add(GetRotation(_currentIndex));
                for (var i = 1; i <= SmoothAverage; i++)
                {
                    if (_currentIndex + i < _cache.entries.Length)
                        rotations.Add(GetRotation(_currentIndex + i));

                    if (_currentIndex - i >= 0)
                        rotations.Add(GetRotation(_currentIndex - i));
                }
                VRListener.CurrentRotation = MathUtility.Average(rotations);
            }
        }

        private Vector3 GetPosition(int index)
        {
            switch (TrackedDevice)
            {
                case PlaybackTrackedDevice.Head:
                    return _cache.entries[index].HeadPos;
                case PlaybackTrackedDevice.LeftHand:
                    return _cache.entries[index].LHPos;
                case PlaybackTrackedDevice.RightHand:
                    return _cache.entries[index].RHPos;
            }
            return Vector3.zero;
        }

        private Quaternion GetRotation(int index)
        {
            switch (TrackedDevice)
            {
                case PlaybackTrackedDevice.Head:
                    return _cache.entries[index].HeadRot;
                case PlaybackTrackedDevice.LeftHand:
                    return _cache.entries[index].LHRot;
                case PlaybackTrackedDevice.RightHand:
                    return _cache.entries[index].RHRot;
            }
            return Quaternion.identity;
        }

        private void OnDisable()
        {
            _cache = new RecordedFile();
            VRListener.UpdateRequested -= Update;
        }
    }
}
