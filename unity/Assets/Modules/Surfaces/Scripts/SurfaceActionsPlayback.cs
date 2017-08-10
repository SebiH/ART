using Assets.Modules.Core;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class SurfaceActionsPlayback : MonoBehaviour
    {
        public string Filename = "actions.json";

        [Serializable]
        private struct RecordedFile
        {
            public SurfaceActionsRecorder.RecordedCommand[] entries;
        }
        private RecordedFile _cache;
        private int _currentIndex = 0;

        private void OnEnable()
        {
            var text = File.ReadAllText(FileUtility.GetPath(Filename));
            _cache = JsonUtility.FromJson<RecordedFile>(text);
        }

        private void OnDisable()
        {
            _cache = new RecordedFile();
        }

        private void Update()
        {
            while (_currentIndex < _cache.entries.Length && _cache.entries[_currentIndex].time <= Time.unscaledTime)
            {
                var current = _cache.entries[_currentIndex];
                RemoteSurfaceConnection.HandlePacketDebug(Globals.DefaultSurfaceName, current.cmd, current.payload);

                _currentIndex++;
            }
        }
    }
}
