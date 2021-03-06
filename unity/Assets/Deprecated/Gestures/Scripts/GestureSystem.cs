using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.GestureControl
{
    /// <summary>
    /// *One* GestureSystem per scene. This script behaves similar to EventSystem, which handles GUI elements.
    /// </summary>
    public class GestureSystem : MonoBehaviour
    {
        private static GameObject[] RegisteredLimbs = new GameObject[Enum.GetNames(typeof(InteractionLimb)).Length];

        private static int LimbToInt(InteractionLimb limb)
        {
            // enums are usually just integers with names
            return (int)limb;
        }

        public static void RegisterLimb(InteractionLimb type, GameObject limb)
        {
            var typeIndex = LimbToInt(type);

            if (RegisteredLimbs[typeIndex] != null)
            {
                Debug.LogWarning(String.Format("Already registered limb '{0}'! Overwriting old value..", type.ToString()));
            }

            RegisteredLimbs[typeIndex] = limb;
        }

        public static void DeregisterLimb(InteractionLimb type)
        {
            var typeIndex = LimbToInt(type);
            RegisteredLimbs[typeIndex] = null;
        }

        public static GameObject GetLimb(InteractionLimb type)
        {
            var typeIndex = LimbToInt(type);
            return RegisteredLimbs[typeIndex];
        }


        public static Dictionary<InteractionLimb, GameObject> GetLimbs(InteractionLimb[] limbs)
        {
            var limbMap = new Dictionary<InteractionLimb, GameObject>();

            foreach (var limb in limbs)
            {
                limbMap.Add(limb, GetLimb(limb));
            }

            return limbMap;
        }



        private static ICollection<GestureBase> RegisteredGestures = new List<GestureBase>();

        public static void RegisterGesture(GestureBase gesture)
        {
            RegisteredGestures.Add(gesture);
        }

        public static void DeregisterGesture(GestureBase gesture)
        {
            Debug.Assert(RegisteredGestures.Contains(gesture));
            RegisteredGestures.Remove(gesture);
        }






        void Update()
        {
            foreach (var gesture in RegisteredGestures)
            {
                try
                {
                    //var triggered = gesture.CheckConditions();
                    gesture.CheckConditions();
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }

                //if (triggered)
                //{
                //    Debug.Log(String.Format("Triggered gesture {0}", gesture.GetName()));
                //}
            }
        }
    }
}
