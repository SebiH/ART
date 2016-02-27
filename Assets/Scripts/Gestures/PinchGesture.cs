using System;
using GestureControl;
using UnityEngine;

public class PinchGesture : GestureBase
{
    public bool TriggerOnLeftHand = true;
    public bool TriggerOnRightHand = true;

    private bool IsTriggeredLeft = false;
    private bool IsTriggeredRight = false;

    public override string GetName()
    {
        return "PinchGesture";
    }

    public override bool CheckConditions()
    {
        bool hasTriggered = false;

        if (TriggerOnLeftHand)
        {
            var leftThumb = GestureSystem.GetLimb(InteractionLimb.LeftThumbTip);
            var leftIndex = GestureSystem.GetLimb(InteractionLimb.LeftIndexTip);
        }


        if (TriggerOnRightHand)
        {
            var rightThumb = GestureSystem.GetLimb(InteractionLimb.RightThumbTip);
            var rightIndex = GestureSystem.GetLimb(InteractionLimb.RightIndexTip);

            if (GestureUtil.CollidesWith(rightThumb, rightIndex))
            {
                hasTriggered = true;

                if (!IsTriggeredRight)
                {
                    Debug.Log("Triggered on right hand");
                    IsTriggeredRight = true;
                }
            }
            else if (IsTriggeredRight)
            {
                Debug.Log("No longer triggered on right hand");
                IsTriggeredRight = false;
            }
        }

        return hasTriggered;
    }
}
