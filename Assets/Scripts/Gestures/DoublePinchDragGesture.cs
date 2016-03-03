using Assets.Scripts.GestureControl;
using UnityEngine;

namespace Assets.Scripts.Gestures
{
    public class DoublePinchDragGesture : GestureBase
    {
        public float ActivationDistance = 0.03f;

        private bool IsGestureActive;

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
                if (GestureUtil.CollidesWith(leftThumb, leftIndex) && GestureUtil.CollidesWith(rightThumb, rightIndex))
                {
                    // gesture still active!
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
                if (GestureUtil.IsInProximity(ActivationDistance, new[] { leftThumb, leftIndex, rightThumb, rightIndex }))
                {
                    IsGestureActive = true;
                    RaiseGestureStartEvent();
                }
            }


            return IsGestureActive;
        }
    }
}
