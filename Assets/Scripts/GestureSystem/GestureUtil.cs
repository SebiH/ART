using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GestureControl
{
    public static class GestureUtil
    {
        public static bool CollidesWith(GameObject o1, GameObject o2)
        {
            var c1 = o1.GetComponent<Collider>();
            var c2 = o2.GetComponent<Collider>();

            if (c1 == null || c2 == null)
            {
                Debug.LogError("Could not compare collision: null object detected");
                return false;
            }

            return c1.bounds.Intersects(c2.bounds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distanceThreshold">Distance threshold in centimeters</param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static bool IsInProximity(double distanceThreshold, IEnumerable<GameObject> objects)
        {
            // build the average position to apply distanceThreshold to
            Vector3 averagePosition = Vector3.zero;
            int objectsCount = 0;

            foreach (var gameObject in objects)
            {
                averagePosition += gameObject.transform.position;
                objectsCount++;
            }

            if (objectsCount == 0)
            {
                Debug.LogError("Tried to call IsInProximity without objects!");
                return false;
            }

            averagePosition = averagePosition / objectsCount;


            // check if all objects are within the threshold to the average position of all objects
            foreach (var gameObject in objects)
            {
                if (Vector3.Distance(gameObject.transform.position, averagePosition) > distanceThreshold)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
