using GestureControl;
using System;
using UnityEngine;

namespace Assets.Scripts.Gestures
{
    public class PalmUpGesture : GestureBase
    {
        /// <summary>
        /// From which hand the gesture should trigger
        /// </summary>
        public Hand TriggerHand;

        /// <summary>
        /// Threshold how much the palm has to point upwards to activate this gesture, in degrees.
        /// </summary>
        public float ActivateThreshold = 25f;

        /// <summary>
        /// Threshold how much the palm can deviate from the upwards vector, before this gesture deactivates, in degrees.
        /// </summary>
        public float DeactivateThreshold = 50f;


        private bool IsGestureActive = false;

        public override string GetName()
        {
            return "Palm Up Gesture";
        }

        public override bool CheckConditions()
        {
            if (TriggerHand == Hand.Both)
            {
                var leftPalm = GestureSystem.GetLimb(InteractionLimb.LeftPalm);
                var leftStatus = CheckStatus(leftPalm);

                var rightPalm = GestureSystem.GetLimb(InteractionLimb.RightPalm);
                var rightStatus = CheckStatus(rightPalm);

                if (leftStatus == GestureStatus.Starting && rightStatus == GestureStatus.Starting)
                {
                    IsGestureActive = true;
                    OnGestureStart();
                }
                else if (leftStatus == GestureStatus.Active && rightStatus == GestureStatus.Active)
                {
                    OnGestureHold();
                }
                else if (leftStatus == GestureStatus.Stopping || rightStatus == GestureStatus.Stopping)
                {
                    IsGestureActive = false;
                    OnGestureEnd();
                }
            }
            else
            {
                var limbType = (TriggerHand == Hand.Left) ? InteractionLimb.LeftPalm : InteractionLimb.RightPalm;
                var palm = GestureSystem.GetLimb(limbType);
                var status = CheckStatus(palm);

                switch (status)
                {
                    case GestureStatus.Starting:
                        IsGestureActive = true;
                        OnGestureStart();
                        break;

                    case GestureStatus.Active:
                        OnGestureHold();
                        break;

                    case GestureStatus.Stopping:
                        IsGestureActive = false;
                        OnGestureEnd();
                        break;

                    case GestureStatus.Inactive:
                        // do nothing
                        break;

                    default:
                        throw new InvalidOperationException("Unknown enum for GestureStatus");
                }
            }

            return IsGestureActive;
        }

        private bool ShouldGestureStart(GameObject limb)
        {
            return Vector3.Angle(-limb.transform.up, Vector3.up) <= ActivateThreshold;
        }

        private bool ShouldGestureStop(GameObject limb)
        {
            return Vector3.Angle(-limb.transform.up, Vector3.up) >= DeactivateThreshold;
        }

        private GestureStatus CheckStatus(GameObject limb)
        {
            GestureStatus status;

            if (!IsGestureActive)
            {
                if (ShouldGestureStart(limb))
                {
                    status = GestureStatus.Starting;
                }
                else
                {
                    status = GestureStatus.Inactive;
                }
            }
            else
            {
                if (ShouldGestureStop(limb))
                {
                    status = GestureStatus.Stopping;
                }
                else
                {
                    status = GestureStatus.Active;
                }
            }

            return status;
        }
    }
}
