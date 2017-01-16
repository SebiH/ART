using System.Linq;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class Graph : MonoBehaviour
    {
        public int Id;
        public RemoteDataProvider DataProvider { get; set; }

        public delegate void DataChangeHandler();
        public event DataChangeHandler OnDataChange;

        public DataPoint2D[] Data { get; private set; }

        private string _currentDimX = null;
        private DataPoint[] _dataX = null;
        private string _currentDimY = null;
        private DataPoint[] _dataY = null;

        void OnEnable()
        {
            Data = null;
        }

        void OnDisable()
        {

        }

        public void SetData(string dimX, string dimY)
        {
            if (_currentDimX != dimX || _currentDimY != dimY)
            {
                Data = null;
            }

            if (_currentDimX != dimX)
            {
                _dataX = null;
                _currentDimX = dimX;

                if (!string.IsNullOrEmpty(dimX))
                {
                    DataProvider.LoadDataAsync(dimX, OnDimXLoaded);
                }
            }

            if (_currentDimY != dimY)
            {
                _dataY = null;
                _currentDimY = dimY;

                if (!string.IsNullOrEmpty(dimY))
                {
                    DataProvider.LoadDataAsync(dimY, OnDimYLoaded);
                }
            }
        }

        private void OnDimXLoaded(string dimension, DataPoint[] dataX)
        {
            // sanity check in case dimension changed while loading data
            if (dimension == _currentDimX)
            {
                _dataX = dataX.OrderBy(d => d.Index).ToArray();
                BuildData();
            }
        }

        private void OnDimYLoaded(string dimension, DataPoint[] dataY)
        {
            // sanity check in case dimension changed while loading data
            if (dimension == _currentDimY)
            {
                _dataY = dataY.OrderBy(d => d.Index).ToArray();
                BuildData();
            }
        }

        private void BuildData()
        {
            if (_dataX != null && _dataY != null)
            {
                Debug.Assert(_dataX.Length == _dataY.Length);

                Data = new DataPoint2D[_dataX.Length];

                for (int i = 0; i < _dataX.Length; i++)
                {
                    Debug.Assert(_dataX[i].Index == _dataY[i].Index);
                    Data[i] = new DataPoint2D
                    {
                        Index = _dataX[i].Index,
                        ValueX = _dataX[i].Value,
                        ValueY = _dataY[i].Value
                    };
                }
            }
            else
            {
                Data = null;
            }

            if (OnDataChange != null)
            {
                OnDataChange();
            }
        }
    }
}
