using Assets.Modules.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Deprecated.Graph
{
    public class RemoteDataProvider : DataProvider
    {
        public string ServerAddress = "http://localhost:81/";

        // TODO hack
        public bool IsReady = false;

        private Dictionary<string, float[]> _graphData = new Dictionary<string, float[]>();

        void OnEnable()
        {
            StartCoroutine(LoadData());
        }

        void OnDisable()
        {

        }

        private IEnumerator LoadData()
        {
            var dimensionWebRequest = new WWW(ServerAddress + "api/graph/dimensions");
            yield return dimensionWebRequest;

            WebRequestHelper.WebResult dimensionResult;
            WebRequestHelper.Instance.PerformWebRequest("dimensions", dimensionWebRequest, out dimensionResult);

            foreach (var dim in JsonUtility.FromJson<DimensionResponse>(dimensionResult.text).dimensions)
            {
                var dataRequestForm = new WWWForm();
                dataRequestForm.AddField("dimension", dim);

                var dataWebRequest = new WWW(ServerAddress + "api/graph/data", dataRequestForm);
                yield return dataWebRequest;

                WebRequestHelper.WebResult dataResult;
                WebRequestHelper.Instance.PerformWebRequest("data:" + dim, dataWebRequest, out dataResult);

                var dimData = JsonUtility.FromJson<DataResponse>(dataResult.text).data;
                _graphData[dim] = dimData;
            }

            IsReady = true;
        }

        public override float[,] GetData()
        {
            throw new NotImplementedException();
        }

        public override Vector2[] GetDimData(string dimX, string dimY)
        {
            // TODO: hack
            Debug.Assert(IsReady);

            var dataX = _graphData[dimX];
            var dataY = _graphData[dimY];
            Debug.Assert(dataX.Length == dataY.Length);

            var graphData = new Vector2[dataX.Length];

            for (int i = 0; i < dataX.Length; i++)
            {
                graphData[i] = new Vector2(dataX[i], dataY[i]);
            }

            return graphData;
        }




        private struct DimensionResponse
        {
            public string[] dimensions;
        }
        
        private struct DataResponse
        {
            public float[] data;
        }
    }
}
