using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class RemoteDataProvider : MonoBehaviour
    {
        private Dictionary<string, DataPoint[]> _loadedData = new Dictionary<string, DataPoint[]>();
        private Dictionary<string, List<Action<string, DataPoint[]>>> _loadOperations = new Dictionary<string, List<Action<string, DataPoint[]>>>();

        public virtual void LoadDataAsync(string dimension, Action<string, DataPoint[]> onDataLoaded)
        {
            if (_loadedData.ContainsKey(dimension))
            {
                onDataLoaded(dimension, _loadedData[dimension]);
            }
            else if (_loadOperations.ContainsKey(dimension))
            {
                _loadOperations[dimension].Add(onDataLoaded);
            }
            else
            {
                _loadOperations.Add(dimension, new List<Action<string, DataPoint[]>>());
                _loadOperations[dimension].Add(onDataLoaded);
                StartCoroutine(LoadData(dimension));
            }
        }


        private IEnumerator LoadData(string dimension)
        {
            var dataRequestForm = new WWWForm();
            dataRequestForm.AddField("dimension", dimension);

            var dataWebRequest = new WWW(String.Format("{0}:{1}/api/graph/data", Globals.SurfaceServerIp, Globals.SurfaceWebPort), dataRequestForm);
            yield return dataWebRequest;

            var response = JsonUtility.FromJson<DataResponse>(dataWebRequest.text);
            var dimData = response.data;
            var range = response.domain.max - response.domain.min;

            var dataPoints = new DataPoint[dimData.Length];
            for (int i = 0; i < dimData.Length; i++)
            {
                dataPoints[i] = new DataPoint { Index = i, Value = (dimData[i] - response.domain.min) / range - 0.5f };
            }

            _loadedData[dimension] = dataPoints;

            foreach (var onDataLoaded in _loadOperations[dimension])
            {
                onDataLoaded(dimension, dataPoints);
            }
            _loadOperations.Remove(dimension);
        }

        [Serializable]
        private class DataResponse
        {
            public float[] data = null;
            public DataDomain domain = null;
            public string name = "";
            public bool isMetric = true;
            public DataMapping[] mappings = new DataMapping[0];
        }

        [Serializable]
        private class DataDomain
        {
            public float min = 0;
            public float max = 1;
        }

        [Serializable]
        private class DataMapping
        {
            public float value = 0f;
            public string name = "";
        }
    }
}
