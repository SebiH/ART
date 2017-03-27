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
        private Scale _scaleX = new NullScale();
        public Dimension DimX
        {
            get { return _dimX; }
            set
            {
                if (_dimX != value)
                {
                    _scaleX = GetScale(value);
                    _dimX = value;
                    TriggerDataChange();
                }
            }
        }
        public Scale ScaleX { get { return _scaleX; } }

        private Dimension _dimY;
        private Scale _scaleY = new NullScale();
        public Dimension DimY
        {
            get { return _dimY; }
            set
            {
                if (_dimY != value)
                {
                    _scaleY = GetScale(value);
                    _dimY = value;
                    TriggerDataChange();
                }
            }
        }
        public Scale ScaleY { get { return _scaleY; } }

        public void SetDimensions(Dimension x, Dimension y)
        {
            bool hasChanged = false;
            if (x != _dimX)
            {
                _scaleX = GetScale(x);
                hasChanged = true;
                _dimX = x;
            }

            if (y != _dimY)
            {
                _scaleY = GetScale(y);
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
            if (_dataCache == null)
            {
                Debug.Assert(_dimX != null && _dimY != null, "Tried retrieving data from graph with null dimensions");
                return new Vector2(_scaleX.Convert(_dimX.Data[index]), _scaleY.Convert(_dimY.Data[index]));
            }
            else
            {
                return _dataCache[index];
            }
        }

        public Vector2[] GetDataPosition()
        {
            if (_dataCache == null)
            {
                Debug.Assert(_dimX != null && _dimY != null, "Tried retrieving data from graph with null dimensions");
                _dataCache = new Vector2[Globals.DataPointsCount];
                for (var i = 0; i < _dataCache.Length; i++)
                {
                    _dataCache[i] = new Vector2(_scaleX.Convert(_dimX.Data[i]), _scaleY.Convert(_dimY.Data[i]));
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

        private Scale GetScale(Dimension dim)
        {
            if (dim is CategoricalDimension)
            {
                return new CategoryScale();
            }
            else if (dim is MetricDimension)
            {
                return new LinearScale();
            }
            else
            {
                return new NullScale();
            }
        }
    }
}
