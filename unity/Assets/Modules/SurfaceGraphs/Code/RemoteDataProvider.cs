using Assets.Modules.Core;
using Assets.Modules.Graphs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    public class RemoteDataProvider
    {
        private Dictionary<string, Dimension> _loadedData = new Dictionary<string, Dimension>();
        private Dictionary<string, List<Action<Dimension>>> _loadOperations = new Dictionary<string, List<Action<Dimension>>>();

        public virtual void LoadDataAsync(string dimensionName, Action<Dimension> onDataLoaded)
        {
            if (_loadedData.ContainsKey(dimensionName))
            {
                // already cached
                onDataLoaded(_loadedData[dimensionName]);
            }
            else if (_loadOperations.ContainsKey(dimensionName))
            {
                // load operation already active, queue up for response
                _loadOperations[dimensionName].Add(onDataLoaded);
            }
            else
            {
                // start new request
                _loadOperations.Add(dimensionName, new List<Action<Dimension>>());
                _loadOperations[dimensionName].Add(onDataLoaded);
                GameLoop.Instance.StartCoroutine(LoadData(dimensionName));
            }
        }


        private IEnumerator LoadData(string dimensionName)
        {
            var dataRequestForm = new WWWForm();
            dataRequestForm.AddField("dimension", dimensionName);

            var dataWebRequest = new WWW(String.Format("{0}:{1}/api/graph/data", Globals.SurfaceServerIp, Globals.SurfaceWebPort), dataRequestForm);
            yield return dataWebRequest;

            var response = JsonUtility.FromJson<DataResponse>(dataWebRequest.text);
            var dimData = response.data;
            var range = response.domain.max - response.domain.min;

            var data = new float[dimData.Length];
            if (response.isMetric)
            {
                for (int i = 0; i < dimData.Length; i++)
                {
                    data[i] = (dimData[i] - response.domain.min) / range - 0.5f;
                }
            }
            else
            {
                for (int i = 0; i < dimData.Length; i++)
                {
                    data[i] = (dimData[i] + 1 - response.domain.min) / (range + 2) - 0.5f;
                }
            }

            Dimension dimension;

            if (response.isMetric)
            {
                dimension = new MetricDimension();
            }
            else
            {
                var catDimension = new CategoricalDimension();
                foreach (var mapping in response.mappings)
                {
                    catDimension.Mappings.Add(new CategoricalDimension.Mapping { Name = mapping.name, Value = mapping.value });
                }
                dimension = catDimension;
            }

            dimension.Data = data;
            dimension.DisplayName = dimensionName;
            dimension.DomainMin = response.domain.min;
            dimension.DomainMax = response.domain.max;

            dimension.RebuildData();

            foreach (var onDataLoaded in _loadOperations[dimensionName])
            {
                onDataLoaded(dimension);
            }
            _loadOperations.Remove(dimensionName);
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
