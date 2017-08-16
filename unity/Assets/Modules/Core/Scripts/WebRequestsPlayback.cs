using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Core
{
    public class WebRequestsPlayback : WebRequestHelper
    {
        public CurrentFilename file;

        [Serializable]
        private struct RecordedFile
        {
            public WebRequestsRecorder.RecordedRequest[] entries;
        }
        private RecordedFile _cache;

        private void OnEnable()
        {
            WebRequestHelper.Instance = this;

            var text = File.ReadAllText(FileUtility.GetPath(file.Filename + ".webrequests.json"));
            _cache = JsonUtility.FromJson<RecordedFile>(text);
        }

        private void OnDisable()
        {
            _cache = new RecordedFile();
        }

        public override void PerformWebRequest(string identifier, WWW request, out WebResult result)
        {
            result = new WebResult();
            foreach (var entry in _cache.entries)
            {
                if (entry.id == identifier)
                {
                    result.text = entry.result;
                    return;
                }
            }
        }
    }
}
