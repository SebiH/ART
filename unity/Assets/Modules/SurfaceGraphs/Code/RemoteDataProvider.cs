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
            Dimension dimension;

            if (response.isMetric)
            {
                var metricDimension = new MetricDimension();
                metricDimension.PossibleTicks = response.ticks;
                metricDimension.IsTimeBased = response.isTimeBased;
                metricDimension.TimeFormat = response.timeFormat;
                dimension = metricDimension;
            }
            else
            {
                var catDimension = new CategoricalDimension();
                foreach (var mapping in response.mappings)
                {
                    catDimension.Mappings.Add(new Dimension.Mapping { Name = mapping.name, Value = mapping.value });
                }
                dimension = catDimension;
            }

            var data = new Dimension.DimData[response.data.Length];
            for (var i = 0; i < response.data.Length; i++)
            {
                // assuming ids are 0 - data.length
                data[i].Id = int.Parse(response.data[i].id);
                data[i].Value = response.data[i].value;
            }

            dimension.Data = data;
            dimension.DisplayName = response.name;
            dimension.DomainMin = response.domain.min;
            dimension.DomainMax = response.domain.max;
            dimension.HideTicks = response.hideTicks;

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
            public DataValue[] data = null;
            public DataDomain domain = null;
            public string name = "";
            public bool hideTicks = false;
            public bool isMetric = true;
            public bool isTimeBased = false;
            public string timeFormat = "";
            public DataMapping[] mappings = new DataMapping[0];
            public GradientStop[] gradient = new GradientStop[0];
            public float[] ticks = new float[0];
        }

        [Serializable]
        private class DataValue
        {
            public string id = "0";
            public float value = -1;
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

        [Serializable]
        private class GradientStop
        {
            public float stop = 0f;
            public string color = "#FFFFFF";
        }
    }
}
