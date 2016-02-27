using System;
using System.Collections.Generic;
using UnityEngine;

namespace GestureControl
{
    public static class GestureUtil
    {
        public static bool CollidesWith(GameObject o1, GameObject o2)
        {
            var c1 = o1.GetComponent<Collider>();
            var c2 = o2.GetComponent<Collider>();

            return c1.bounds.Intersects(c2.bounds);
        }

        public static bool IsInProximity(double distanceThreshold, IEnumerable<GameObject> objects)
        {
            throw new NotImplementedException();
        }
    }
}
