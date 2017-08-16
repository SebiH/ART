using Assets.Modules.Core;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class SurfaceActionsRecorder : MonoBehaviour
    {
        public CurrentFilename file;
        private StreamWriter _cache;

        [Serializable]
        public struct RecordedCommand
        {
            public float time;
            public string cmd;
            public string payload;
        }

        private void OnEnable()
        {
            _cache = File.CreateText(FileUtility.GetPath(file.Filename + ".actions.json"));
            _cache.AutoFlush = true;
            _cache.WriteLine("{ \"entries\": [");

            RemoteSurfaceConnection.OnCommandReceived += RecordCommand;
        }

        private void RecordCommand(string cmd, string payload)
        {
            var entry = new RecordedCommand
            {
                time = Time.unscaledTime,
                cmd = cmd,
                payload = payload
            };
            _cache.WriteLine(JsonUtility.ToJson(entry) + ",");
        }

        private void OnDisable()
        {
            _cache.WriteLine("]}");
            _cache.Close();
        }
    }
}
