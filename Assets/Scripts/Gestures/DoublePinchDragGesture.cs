using Assets.Scripts.GestureControl;
using System;
using UnityEngine;

namespace Assets.Scripts.Gestures
{
    public class DoublePinchDragGesture : GestureBase, IPositionGesture
    {
        public float ActivationDistance = 0.05f;
        public float DeactivationDistance = 0.02f;

        private bool IsGestureActive;


        // Position of the gesture occurence of the left hand
        private Vector3 GesturePosLeft;

        // Position of the gesture occurence of the right hand
        private Vector3 GesturePosRight;
        
        // Center position of the gesture
        private Vector3 GesturePosCenter;

        public Vector3 GetGesturePosition(Hand hand)
        {
            Vector3 pos = Vector3.zero;

            switch (hand)
            {
                case Hand.Left:
                    pos = GesturePosLeft;
                    break;

                case Hand.Right:
                    pos = GesturePosRight;
                    break;

                case Hand.Both:
                    pos = GesturePosCenter;
                    break;

                default:
                    throw new NotSupportedException("Hand " + hand.ToString() + " is not supported");
            }
            
            return pos;
        }


        public override string GetName()
        {
            return "Double Pinch and Drag Gesture";
        }


        public override bool CheckConditions()
        {
            var leftThumb = GestureSystem.GetLimb(InteractionLimb.LeftThumbTip);
            var leftIndex = GestureSystem.GetLimb(InteractionLimb.LeftIndexTip);

            var rightThumb = GestureSystem.GetLimb(InteractionLimb.RightThumbTip);
            var rightIndex = GestureSystem.GetLimb(InteractionLimb.RightIndexTip);

            if (IsGestureActive)
            {
                // if the gesture is already active, we only need to check if the user is still pinching their fingers together
                var isPinchingLeft = GestureUtil.IsInProximity(DeactivationDistance, new[] { leftThumb, leftIndex });
                var isPinchingRight = GestureUtil.IsInProximity(DeactivationDistance, new[] { rightThumb, rightIndex });

                if (isPinchingLeft && isPinchingRight)
                {
                    // gesture still active
                    GesturePosLeft = GestureUtil.GetCenterPosition(new[] { leftThumb, leftIndex });
                    GesturePosRight = GestureUtil.GetCenterPosition(new[] { rightThumb, rightIndex });
                    GesturePosCenter = GestureUtil.GetCenterPosition(new[] { leftThumb, leftIndex, rightThumb, rightIndex });

                    RaiseGestureActiveEvent();
                }
                else
                {
                    // one of the hands stopped pinching - stop gesture
                    IsGestureActive = false;
                    RaiseGestureStopEvent();
                }

            }
            else
            {
                // gesture not active, so we'll check if the gesture is initiated
                var isPinchingLeft = GestureUtil.CollidesWith(leftThumb, leftIndex);
                var isPinchingRight = GestureUtil.CollidesWith(rightThumb, rightIndex);
                var areHandsTogether = GestureUtil.IsInProximity(ActivationDistance, new[] { leftThumb, leftIndex, rightThumb, rightIndex });


                if (isPinchingLeft && isPinchingRight && areHandsTogether)
                {
                    IsGestureActive = true;

                    GesturePosLeft = GestureUtil.GetCenterPosition(new[] { leftThumb, leftIndex });
                    GesturePosRight = GestureUtil.GetCenterPosition(new[] { rightThumb, rightIndex });
                    GesturePosCenter = GestureUtil.GetCenterPosition(new[] { leftThumb, leftIndex, rightThumb, rightIndex });

                    RaiseGestureStartEvent();
                }
            }


            return IsGestureActive;
        }
    }
}
