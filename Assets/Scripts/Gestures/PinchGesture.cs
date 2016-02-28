using System;
using GestureControl;
using UnityEngine;

public class PinchGesture : GestureBase
{
    public Hand TriggerHand;

    private bool IsGestureActive;

    public override string GetName()
    {
        return "PinchGesture";
    }

    public override bool CheckConditions()
    {
        if (TriggerHand == Hand.Both)
        {
            var leftThumb = GestureSystem.GetLimb(InteractionLimb.LeftThumbTip);
            var leftIndex = GestureSystem.GetLimb(InteractionLimb.LeftIndexTip);
            var leftStatus = CheckStatus(leftThumb, leftIndex);

            var rightThumb = GestureSystem.GetLimb(InteractionLimb.RightThumbTip);
            var rightIndex = GestureSystem.GetLimb(InteractionLimb.RightIndexTip);
            var rightStatus = CheckStatus(rightThumb, rightIndex);

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
            var indexType = (TriggerHand == Hand.Left) ? InteractionLimb.LeftIndexTip : InteractionLimb.RightIndexTip;
            var thumbType = (TriggerHand == Hand.Left) ? InteractionLimb.LeftThumbTip : InteractionLimb.RightThumbTip;

            var index = GestureSystem.GetLimb(indexType);
            var thumb = GestureSystem.GetLimb(thumbType);

            var status = CheckStatus(thumb, index);

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


    private GestureStatus CheckStatus(GameObject thumb, GameObject index)
    {
        GestureStatus status;

        if (GestureUtil.CollidesWith(thumb, index))
        {
            status = (IsGestureActive) ? GestureStatus.Active : GestureStatus.Starting;
        }
        else if (IsGestureActive)
        {
            // does not collide (anymore) but is still marked as active -> deactivate
            status = GestureStatus.Stopping;
        }
        else
        {
            // doesn't collide, nor is it active -> inactive
            status = GestureStatus.Inactive;
        }

        return status;
    }
}
