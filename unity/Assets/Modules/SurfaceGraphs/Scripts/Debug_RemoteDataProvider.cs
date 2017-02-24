using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.SurfaceGraphs
{
    public class Debug_RemoteDataProvider : MonoBehaviour
    {
        private Dictionary<string, DataPoint[]> _rndValues = new Dictionary<string, DataPoint[]>();
        public int NumData = 100;

        public override void LoadDataAsync(string dimension, Action<string, DataPoint[]> onDataLoaded)
        {
            if (!_rndValues.ContainsKey(dimension))
            {
                var rndValues = new DataPoint[NumData];
                for (var i = 0; i < rndValues.Length; i++)
                {
                    rndValues[i] = new DataPoint
                    {
                        Index = i,
                        Value = UnityEngine.Random.Range(-0.5f, 0.5f)
                    };
                }

                _rndValues.Add(dimension, rndValues);
            }

            onDataLoaded(dimension, _rndValues[dimension]);
        }
    }
}
