using Assets.Modules.Core;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class PlaybackVrTracker : MonoBehaviour
    {
        public string Filename;

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
        }

        private void FixedUpdate()
        {
            while (_currentIndex < _cache.entries.Length - 1 && _cache.entries[_currentIndex + 1].Time <= Time.unscaledTime)
            {
                _currentIndex++;
            }

            if (_currentIndex < _cache.entries.Length)
            {
                switch (TrackedDevice)
                {
                    case PlaybackTrackedDevice.Head:
                        VRListener.CurrentPosition = _cache.entries[_currentIndex].HeadPos;
                        VRListener.CurrentRotation = _cache.entries[_currentIndex].HeadRot;
                        VRListener.PoseUpdateTime = _cache.entries[_currentIndex].Time;
                        break;
                    case PlaybackTrackedDevice.LeftHand:
                        VRListener.CurrentPosition = _cache.entries[_currentIndex].LHPos;
                        VRListener.CurrentRotation = _cache.entries[_currentIndex].LHRot;
                        VRListener.PoseUpdateTime = _cache.entries[_currentIndex].Time;
                        break;
                    case PlaybackTrackedDevice.RightHand:
                        VRListener.CurrentPosition = _cache.entries[_currentIndex].RHPos;
                        VRListener.CurrentRotation = _cache.entries[_currentIndex].RHRot;
                        VRListener.PoseUpdateTime = _cache.entries[_currentIndex].Time;
                        break;
                }
            }
        }

        private void OnDisable()
        {
            _cache = new RecordedFile();
        }
    }
}
