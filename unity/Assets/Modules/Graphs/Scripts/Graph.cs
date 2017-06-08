using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class Graph : MonoBehaviour
    {
        public int Id { get; set; }
        public Color Color { get; set; }
        public bool IsSelected { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsNewlyCreated { get; set; }
        public bool IsPickedUp { get; set; }

        public bool IsColored { get; set; }

        public event Action OnDataChange;

        // minor performance improvement
        private Vector2[] _dataCache = null;

        private CategoricalDimension _sortedDimX;
        private Dimension _dimX;
        public Dimension DimX
        {
            get
            {
                if (SortAxis && _dimX != null)
                {
                    if (_sortedDimX == null)
                    {
                        _sortedDimX = SortBy(_dimX, _dimY);
                    }

                    return _sortedDimX;
                }
                else
                {
                    return _dimX;
                }
            }
            set
            {
                if (_dimX != value)
                {
                    _dimX = value;
                    _sortedDimX = null;
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
                    _sortedDimX = null;
                    TriggerDataChange();
                }
            }
        }

        private bool _sortAxis = false;
        public bool SortAxis
        {
            get
            {
                return _sortAxis;
            }
            set
            {
                if (_sortAxis != value)
                {
                    _sortAxis = value;
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
                Debug.Assert(DimX != null && DimY != null, "Tried retrieving data from graph with null dimensions");
                _dataCache = new Vector2[DimX.Data.Length];
                for (var i = 0; i < _dataCache.Length; i++)
                {
                    _dataCache[i] = new Vector2(DimX.ScaledData[i], DimY.ScaledData[i]);
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

        private CategoricalDimension SortBy(Dimension dimX, Dimension dimY)
        {
            if (dimX == null || dimY == null)
            {
                return null;
            }

            var sortedDim = new CategoricalDimension();
            sortedDim.Name = dimX.Name;
            sortedDim.DisplayName = dimX.DisplayName;
            sortedDim.HideTicks = true;
            sortedDim.Ticks = new Dimension.Mapping[0];
            sortedDim.DomainMin = 0;
            sortedDim.DomainMax = dimX.Data.Length;

            
            sortedDim.Data = dimX.Data.ToArray();
            Array.Sort(sortedDim.Data, (d1, d2) => {
                var d1v = dimY.Data[d1.Id].Value;
                var d2v = dimY.Data[d2.Id].Value;

                if (d1v > d2v) { return -1; }
                if (d1v < d2v) { return 1; }
                return 0;
            });

            sortedDim.Mappings = new List<Dimension.Mapping>();

            for (var i = 0; i < dimX.Data.Length; i++)
            {
                sortedDim.Data[i].Value = i;
                //sortedDim.Mappings.Add(new Dimension.Mapping
                //{
                //    Name = "",
                //    Value = i
                //});
            }

            Array.Sort(sortedDim.Data, (d1, d2) =>
            {
                if (d1.Id < d2.Id) { return -1; }
                if (d1.Id > d2.Id) { return 1; }
                return 0;
            });

            sortedDim.RebuildData();

            return sortedDim;
        }
    }
}
