using System;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class Graph : MonoBehaviour
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public Dimension DimX { get; private set; }
        public Dimension DimY { get; private set; }
        public bool IsSelected { get; set; }
        public bool IsNewlyCreated { get; set; }

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

        public void SetDimensions(Dimension dimX, Dimension dimY)
        {
            if (dimX == null || dimY == null)
            {
                var prevData = _data;
                _data = null;
                if (prevData != null && OnDataChange != null)
                {
                    OnDataChange();
                }
            }
            else if (dimX != DimX || dimY != DimY)
            {
                Debug.Assert(dimX.Data.Length == dimY.Data.Length);
                var dataLength = dimX.Data.Length;
                _data = new DataPoint[dataLength];
                for (int i = 0; i < dataLength; i++)
                {
                    _data[i] = new DataPoint { X = dimX.Data[i], Y = dimY.Data[i] };
                }

                if (OnDataChange != null)
                {
                    OnDataChange();
                }
            }
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
