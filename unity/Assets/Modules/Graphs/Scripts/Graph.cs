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
        public event Action OnPositionChange;

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

        private void TriggerDataChange()
        {
            if (OnDataChange != null)
            {
                OnDataChange();
            }
        }
    }
}
