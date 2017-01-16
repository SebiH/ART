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

        public void LoadDataAsync(string dimension, Action<string, DataPoint[]> onDataLoaded)
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


        public IEnumerator LoadData(string dimension)
        {
            var dataRequestForm = new WWWForm();
            dataRequestForm.AddField("dimension", dimension);

            var dataWebRequest = new WWW(String.Format("{0}:{1}/api/graph/data", Globals.SurfaceServerIp, Globals.SurfaceWebPort), dataRequestForm);
            yield return dataWebRequest;

            var dimData = JsonUtility.FromJson<DataResponse>(dataWebRequest.text).data;

            var dataPoints = new DataPoint[dimData.Length];
            for (int i = 0; i < dimData.Length; i++)
            {
                dataPoints[i] = new DataPoint { Index = i, Value = dimData[i] };
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
        }
    }
}
