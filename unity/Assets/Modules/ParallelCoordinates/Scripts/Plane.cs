using Assets.Modules.Graph;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class Plane : MonoBehaviour
    {
        public LineGenerator Generator;

        // set by manager
        public int Id { get; set; }
        public DataProvider Provider { get; set; }
        public string DimensionX { get; private set; }
        public string DimensionY { get; private set; }

        // basically emulating a double linked list
        private Plane _prevPlane;
        private Plane _nextPlane;

        private bool _hasGeneratedLines = false;
        private Vector2[] _data;

        void OnEnable()
        {

        }

        void OnDisable()
        {
            if (_prevPlane != null)
            {
                // TODO: smooth out lines if plane disappears?
                _prevPlane.ConnectTo(_nextPlane);
            }
        }


        public void ConnectTo(Plane next)
        {
            if (next == _nextPlane)
            {
                return;
            }

            if (next == null)
            {
                Generator.ClearLines();
                _hasGeneratedLines = false;
                _nextPlane = null;
                UpdateScale();
            }
            else
            {
                _nextPlane = next;
                _nextPlane.SetPrevPlane(this);
                UpdateScale();

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
        }

        public void SetPosition(float posX)
        {
            transform.localPosition = new Vector3(0, 0, -posX);
            UpdateScale();
            if (_prevPlane != null)
            {
                _prevPlane.UpdateScale();
            }
        }

        private void UpdateScale()
        {
            if (_nextPlane != null)
            {
                var scale = Mathf.Abs(_nextPlane.transform.localPosition.z - transform.localPosition.z);
                Generator.transform.localScale = new Vector3(1, 1, scale);
            }
            else
            {
                Generator.transform.localScale = Vector3.one;
            }
        }

        public void SetPrevPlane(Plane prev)
        {
            _prevPlane = prev;
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



        void OnDrawGizmosSelected()
        {
            if (_nextPlane != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _nextPlane.transform.position);
            }

            if (_prevPlane != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _prevPlane.transform.position);
            }
        }
    }
}
