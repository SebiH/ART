using System;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class Graph : MonoBehaviour
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public bool IsSelected { get; set; }
        public bool IsNewlyCreated { get; set; }

        private Dimension _dimX;
        public Dimension DimX
        {
            get { return _dimX; }
            set { if (_dimX != value) { _dimX = value; BuildData(); } }
        }

        private Dimension _dimY;
        public Dimension DimY
        {
            get { return _dimY; }
            set { if (_dimY != value) { _dimY = value; BuildData(); } }
        }

        // for layouter
        public float Position { get; set; }
        public float Scale { get; set; }
        public float Width { get; set; }
        public bool IsAnimating { get; set; }

        public event Action OnDataChange;

        private struct DataPoint
        {
            public float X;
            public float Y;
        }

        private DataPoint[] _data = null;
        public bool HasData { get { return _data != null; } }
        public int DataLength { get { return _data.Length; } }

        private void BuildData()
        {
            if (_dimX == null || _dimY == null)
            {
                _data = null;
            }
            else
            {
                Debug.Assert(_dimX.Data.Length == _dimY.Data.Length);
                var dataLength = _dimX.Data.Length;
                _data = new DataPoint[dataLength];
                for (int i = 0; i < dataLength; i++)
                {
                    _data[i] = new DataPoint { X = _dimX.Data[i], Y = _dimY.Data[i] };
                }
            }

            if (OnDataChange != null)
            {
                OnDataChange();
            }
        }

        private void OnEnable()
        {
            Scale = 1;
        }

        public Vector3 GetLocalCoordinates(int index)
        {
            if (_data != null)
            {
                var datum = _data[index];
                return new Vector3(-datum.X, datum.Y, 0);
            }

            return Vector3.zero;
        }

        public Vector3 GetWorldCoordinates(int index)
        {
            return transform.TransformPoint(GetLocalCoordinates(index));
        }
    }
}
