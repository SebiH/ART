using Assets.Modules.Graph;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class Plane : MonoBehaviour
    {
        public LineGenerator Generator;

        // set by manager
        public DataProvider Provider { get; set; }
        public string DimensionX { get; private set; }
        public string DimensionY { get; private set; }

        //private Plane _prevPlane;
        private Plane _nextPlane;

        private bool _hasGeneratedLines = false;
        private Vector2[] _data;

        public void ConnectTo(Plane next)
        {
            _nextPlane = next;

            if (_hasGeneratedLines)
            {
                // TODO: get data from Provider once properly implemented?
                Generator.SetEnd(_nextPlane._data);
            }
            else
            {
                Generator.GenerateLines(_data, _nextPlane._data);
                _hasGeneratedLines = true;
            }
        }


        public void SetDimensions(string dimX, string dimY)
        {
            DimensionX = dimX;
            DimensionY = dimY;
            _data = Provider.GetDimData(dimX, dimY);

            if (_hasGeneratedLines)
            {
                Generator.SetStart(_data);
            }
        }
    }
}
