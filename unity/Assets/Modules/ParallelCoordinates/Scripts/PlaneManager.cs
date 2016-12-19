using Assets.Modules.Graph;
using System.Collections.Generic;
using System.Linq;
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
            //// spawn a few planes for debugging
            //for (int i = 0; i < 3; i++)
            //{
            //    CreatePlane(-1, "DUMMY_X", "DUMMY_Y");
            //}
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

        public void CreatePlane(int id, string dimX, string dimY)
        {
            var planeGameObj = Instantiate(PlaneTemplate);
            planeGameObj.transform.parent = transform;
            planeGameObj.transform.localPosition = Vector3.zero;
            planeGameObj.transform.localRotation = Quaternion.identity;

            var plane = planeGameObj.GetComponent<Plane>();
            plane.Id = id;
            plane.Provider = Provider;
            plane.SetDimensions(dimX, dimY);

            Planes.Add(plane);
        }

        public Plane GetPlane(int id)
        {
            return Planes.FirstOrDefault(p => p.Id == id);
        }

        public void RemovePlane(int id)
        {
            var plane = GetPlane(id);
            Planes.Remove(plane);
            Destroy(plane.gameObject);
        }

        public void SetPlaneOrder(int[] ids)
        {
            if (ids.Length == 0)
                return;

            var firstPlane = GetPlane(ids[0]);
            if (firstPlane != null)
            {
                firstPlane.SetPrevPlane(null);
            }

            // -1 because last plane is not connected to anything
            for (int i = 0; i < ids.Length - 1; i++)
            {
                var plane = GetPlane(ids[i]);
                var nextPlane = GetPlane(ids[i + 1]);

                if (plane != null && nextPlane != null)
                {
                    plane.ConnectTo(nextPlane);
                }
            }
        }
    }
}
