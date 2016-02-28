using GestureControl;
using UnityEngine;

namespace Assets.Scripts.Gestures
{
    public class PalmUpGesture : GestureBase
    {
        /// <summary>
        /// True, if this gesture should activate if the *left* palm is facing upwards. 
        /// </summary>
        public bool TriggerOnLeftHand = true;

        /// <summary>
        /// True, if this gesture should activate if the *right* palm is facing upwards. 
        /// </summary>
        public bool TriggerOnRightHand = true;

        /// <summary>
        /// Threshold how much the palm has to point upwards to activate this gesture, in degrees.
        /// </summary>
        public float ActivateThreshold = 25f;

        /// <summary>
        /// Threshold how much the palm can deviate from the upwards vector, before this gesture deactivates, in degrees.
        /// </summary>
        public float DeactivateThreshold = 50f;


        private bool IsLeftHandActive = false;
        private bool IsRightHandActive = false;

        public override string GetName()
        {
            return "Palm Up Gesture";
        }

        public override bool CheckConditions()
        {
            if (TriggerOnLeftHand)
            {
                var leftPalm = GestureSystem.GetLimb(InteractionLimb.LeftPalm);
                var leftCollider = leftPalm.GetComponent<Collider>();

                var handUpVector = -leftCollider.transform.up;
                var worldUpVector = Vector3.up;

                if (!IsLeftHandActive)
                {
                    if (Vector3.Angle(handUpVector, worldUpVector) <= ActivateThreshold)
                    {
                        IsLeftHandActive = true;
                        OnGestureStart();
                    }
                }
                else
                {
                    if (Vector3.Angle(handUpVector, worldUpVector) >= DeactivateThreshold)
                    {
                        IsLeftHandActive = false;
                        OnGestureEnd();
                    }
                    else
                    {
                        OnGestureHold();
                    }
                }
            }


            if (TriggerOnRightHand)
            {
                // TODO - analog to lefthand
            }

            return false;
        }
    }
}
