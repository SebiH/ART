using Assets.Modules.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class PlaneManager : MonoBehaviour
    {
        public GameObject PlaneTemplate;
        public DataProvider Provider;

        public List<GameObject> Planes
        {
            get; private set;
        }

        void OnEnable()
        {
            Planes = new List<GameObject>();

#if UNITY_EDITOR
            // spawn a few planes for debugging
            for (int i = 0; i < 3; i++)
            {
                SpawnPlane("DUMMY_X", "DUMMY_Y");
            }
#endif
        }


        void OnDisable()
        {
            foreach (var plane in Planes)
            {
                Destroy(plane);
            }

            Planes.Clear();
        }

        private int counter = 0;

        void SpawnPlane(string dimX, string dimY)
        {
            var planeGameObj = Instantiate(PlaneTemplate);
            planeGameObj.transform.parent = transform;
            // TODO: proper position
            planeGameObj.transform.localPosition = new Vector3(0, 0, -counter);
            counter++;
            Planes.Insert(0, planeGameObj);

            var plane = planeGameObj.GetComponent<Plane>();
            plane.Provider = Provider;
            plane.SetDimensions(dimX, dimY);

            if (Planes.Count > 1)
            {
                plane.ConnectTo(Planes[1].GetComponent<Plane>());
            }
        }
    }
}
