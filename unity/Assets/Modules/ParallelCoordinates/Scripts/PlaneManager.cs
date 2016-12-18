using Assets.Modules.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class PlaneManager : MonoBehaviour
    {
        public GameObject PlaneTemplate;
        public DataProvider Provider;

        public List<Plane> Planes
        {
            get; private set;
        }

        void OnEnable()
        {
            Planes = new List<Plane>();

#if UNITY_EDITOR
            // spawn a few planes for debugging
            for (int i = 0; i < 3; i++)
            {
                CreatePlane(-1, "DUMMY_X", "DUMMY_Y");
            }
#endif
        }


        void OnDisable()
        {
            while (Planes.Count > 0)
            {
                RemovePlane(Planes[0].Id);
                Planes.RemoveAt(0);
            }
        }

        private int counter = 0;

        public void CreatePlane(int id, string dimX, string dimY)
        {
            var planeGameObj = Instantiate(PlaneTemplate);
            planeGameObj.transform.parent = transform;
            // TODO: proper position
            planeGameObj.transform.localPosition = new Vector3(0, 0, -counter);
            planeGameObj.transform.localRotation = Quaternion.identity;
            counter++;

            var plane = planeGameObj.GetComponent<Plane>();
            plane.Id = id;
            plane.Provider = Provider;
            plane.SetDimensions(dimX, dimY);

            Planes.Insert(0, plane);

            if (Planes.Count > 1)
            {
                plane.ConnectTo(Planes[1].GetComponent<Plane>());
            }
        }

        public void RemovePlane(int id)
        {
            // TODO.
        }

        public void SetPlaneOrder(int[] ids)
        {
            // TODO.
        }
    }
}
