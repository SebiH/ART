using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.ParallelCoordinates
{
    public class PlaneManager : MonoBehaviour
    {
        public GameObject PlaneTemplate;
        public int DataDimSize = 20;

        public List<GameObject> Planes
        {
            get; private set;
        }

        private List<Vector2[]> _dummyData = new List<Vector2[]>();

        void OnEnable()
        {
            Planes = new List<GameObject>();

            for (int i = 0; i < 10; i++)
            {
                var data = new Vector2[DataDimSize];
                for (var j = 0; j < DataDimSize; j++)
                {
                    data[j] = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                }

                _dummyData.Add(data);
            }

#if UNITY_EDITOR
            // spawn a few planes for debugging
            for (int i = 0; i < 3; i++)
            {
                SpawnPlane(i + 1, i);
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


        void SpawnPlane(int startDataIndex, int endDataIndex)
        {
            if (Planes.Count == 0)
            {
                // must always have a 'last' plane without data
                Planes.Add(CreatePlane(endDataIndex));
            }

            var plane = CreatePlane(startDataIndex);
            // always insert in front, so that connections between data remains intact
            Planes.Insert(0, plane);

            var planeData = plane.GetComponent<PlaneData>();
            planeData.SetData(_dummyData[startDataIndex], _dummyData[endDataIndex]);
        }

        GameObject CreatePlane(int position)
        {
            var plane = Instantiate(PlaneTemplate);
            plane.transform.parent = transform;
            plane.transform.localPosition = new Vector3(0, 0, -position);

            return plane;
        }
    }
}
