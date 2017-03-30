using Assets.Modules.Core;
using System;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class Graph : MonoBehaviour
    {
        public int Id { get; set; }
        public Color Color { get; set; }
        public bool IsColored { get; set; }
        public bool IsSelected { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsNewlyCreated { get; set; }

        public event Action OnDataChange;

        // minor performance improvement
        private Vector2[] _dataCache = null;

        private Dimension _dimX;
        public Dimension DimX
        {
            get { return _dimX; }
            set
            {
                if (_dimX != value)
                {
                    _dimX = value;
                    TriggerDataChange();
                }
            }
        }

        private Dimension _dimY;
        public Dimension DimY
        {
            get { return _dimY; }
            set
            {
                if (_dimY != value)
                {
                    _dimY = value;
                    TriggerDataChange();
                }
            }
        }

        public void SetDimensions(Dimension x, Dimension y)
        {
            bool hasChanged = false;
            if (x != _dimX)
            {
                hasChanged = true;
                _dimX = x;
            }

            if (y != _dimY)
            {
                hasChanged = true;
                _dimY = y;
            }

            if (hasChanged)
            {
                TriggerDataChange();
            }
        }

        public Vector2 GetDataPosition(int index)
        {
            Debug.Assert(_dimX != null && _dimY != null, "Tried retrieving data from graph with null dimensions");
            return new Vector2(_dimX.ScaledData[index], _dimY.ScaledData[index]);
        }

        public Vector2[] GetDataPosition()
        {
            if (_dataCache == null)
            {
                Debug.Assert(_dimX != null && _dimY != null, "Tried retrieving data from graph with null dimensions");
                _dataCache = new Vector2[Globals.DataPointsCount];
                for (var i = 0; i < _dataCache.Length; i++)
                {
                    _dataCache[i] = new Vector2(_dimX.ScaledData[i], _dimY.ScaledData[i]);
                }
            }

            return _dataCache;
        }

        private void TriggerDataChange()
        {
            _dataCache = null;

            if (OnDataChange != null)
            {
                OnDataChange();
            }
        }
    }
}
