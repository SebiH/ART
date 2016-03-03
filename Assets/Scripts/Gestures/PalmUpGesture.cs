using Assets.Scripts.GestureControl;
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

        /// <summary>
        /// Determines if the gesture activates if the palm faces upwards (false), or if it activates if it faces the user
        /// </summary>
        public bool ShouldPalmFaceUserInstead = false;

        /// <summary>
        /// Needed if ShouldPalmFaceUserInstead is true
        /// </summary>
        public Camera UserCamera;


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
                    RaiseGestureStartEvent();
                }
                else if (leftStatus == GestureStatus.Active && rightStatus == GestureStatus.Active)
                {
                    RaiseGestureActiveEvent();
                }
                else if (leftStatus == GestureStatus.Stopping || rightStatus == GestureStatus.Stopping)
                {
                    IsGestureActive = false;
                    RaiseGestureStopEvent();
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
                        RaiseGestureStartEvent();
                        break;

                    case GestureStatus.Active:
                        RaiseGestureActiveEvent();
                        break;

                    case GestureStatus.Stopping:
                        IsGestureActive = false;
                        RaiseGestureStopEvent();
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

        private Vector3 GetActivationVector(GameObject limb)
        {
            if (ShouldPalmFaceUserInstead)
            {
                return UserCamera.transform.position - limb.transform.position ;
            }
            else
            {
                return Vector3.up;
            }
        }

        private bool ShouldGestureStart(GameObject limb)
        {
            Vector3 activationVector = GetActivationVector(limb);
            return Vector3.Angle(-limb.transform.up, activationVector) <= ActivateThreshold;
        }

        private bool ShouldGestureStop(GameObject limb)
        {
            Vector3 activationVector = GetActivationVector(limb);
            return Vector3.Angle(-limb.transform.up, activationVector) >= DeactivateThreshold;
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
