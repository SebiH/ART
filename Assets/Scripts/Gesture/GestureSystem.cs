using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Assets.Scripts.Gesture
{
    public class GestureSystem : MonoBehaviour
    {
        private static GameObject[] RegisteredLimbs = new GameObject[Enum.GetNames(typeof(InteractionLimb)).Length];

        public static void RegisterLimb(InteractionLimb type, GameObject limb)
        {
            // enums are usually just integers with names
            var typeIndex = (int)type;

            if (RegisteredLimbs[typeIndex] != null)
            {
                Debug.LogWarning(String.Format("Already registered limb '{0}'! Overwriting old value..", type.ToString()));
            }

            RegisteredLimbs[typeIndex] = limb;
        }


        public static GameObject GetLimb(InteractionLimb type)
        {
            // enums are usually just integers with names
            var typeIndex = (int)type;

            if (RegisteredLimbs[typeIndex] == null)
            {
                Debug.LogWarning(String.Format("Tried to use unregistered limb of type '{0}'!", type.ToString()));
            }

            return RegisteredLimbs[typeIndex];
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
                var triggered = gesture.CheckConditions();
                if (triggered)
                {
                    Debug.Log(String.Format("Triggered gesture {0}", gesture.GetName()));
                }
            }
        }
    }
}
