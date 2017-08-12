using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Core
{
    public class WebRequestsRecorder : WebRequestHelper
    {
        public string Filename = "webrequests.json";
        private StreamWriter _cache;

        private void OnEnable()
        {
            Instance = this;

            _cache = File.CreateText(FileUtility.GetPath(Filename));
            _cache.AutoFlush = true;
            _cache.WriteLine("{ \"entries\": [");
        }

        private void OnDisable()
        {
            _cache.WriteLine("]}");
            _cache.Close();
        }

        public override void PerformWebRequest(string identifier, WWW request, out WebResult result)
        {
            _cache.WriteLine(JsonUtility.ToJson(new RecordedRequest
            {
                id = identifier,
                result = request.text
            }) + ",");

            result = new WebResult();
            result.text = request.text;
        }



        [Serializable]
        public struct RecordedRequest
        {
            public string id;
            public string result;
        }

    }
}
