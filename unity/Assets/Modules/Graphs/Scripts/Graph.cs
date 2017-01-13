using UnityEngine;

namespace Assets.Modules.Graphs
{
    public class Graph : MonoBehaviour
    {
        public int Id;
        public RemoteDataProvider DataProvider { get; set; }

        private DataPoint2D[] _data = null;
        private string _currentDimX = null;
        private string _currentDimY = null;

        void OnEnable()
        {

        }

        void OnDisable()
        {

        }

        public void SetData(string dimX, string dimY)
        {

        }
    }
}
