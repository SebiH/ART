using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;
using System;
using UnityEngine;

public class PinchGesture : GestureBase, IPositionGesture
{
    public Hand TriggerHand;

    private bool IsGestureActive;
    private Vector3 _activePosition;

    public override string GetName()
    {
        return "PinchGesture";
    }

    public Vector3 GetGesturePosition(Hand hand)
    {
        return _activePosition;
    }

    public override bool CheckConditions()
    {
        if (TriggerHand == Hand.Either)
        {
            var leftThumb = GestureSystem.GetLimb(InteractionLimb.LeftThumbTip);
            var leftIndex = GestureSystem.GetLimb(InteractionLimb.LeftIndexTip);

            var rightThumb = GestureSystem.GetLimb(InteractionLimb.RightThumbTip);
            var rightIndex = GestureSystem.GetLimb(InteractionLimb.RightIndexTip);

            if (leftThumb == null || leftIndex == null || rightThumb == null || rightIndex == null)
            {
                return IsGestureActive;
            }

            var leftStatus = CheckStatus(leftThumb, leftIndex);
            var rightStatus = CheckStatus(rightThumb, rightIndex);

            if (leftStatus == GestureStatus.Starting || rightStatus == GestureStatus.Starting)
            {
                if (leftStatus == GestureStatus.Starting)
                    _activePosition = leftIndex.transform.position;
                else
                    _activePosition = rightIndex.transform.position;

                IsGestureActive = true;
                RaiseGestureStartEvent();
            }
            else if (leftStatus == GestureStatus.Active || rightStatus == GestureStatus.Active)
            {
                if (leftStatus == GestureStatus.Active)
                    _activePosition = leftIndex.transform.position;
                else
                    _activePosition = rightIndex.transform.position;

                RaiseGestureActiveEvent();
            }
            else if (leftStatus == GestureStatus.Stopping && rightStatus == GestureStatus.Stopping)
            {
                IsGestureActive = false;
                RaiseGestureStopEvent();
            }
        }
        else if (TriggerHand == Hand.Both)
        {
            var leftThumb = GestureSystem.GetLimb(InteractionLimb.LeftThumbTip);
            var leftIndex = GestureSystem.GetLimb(InteractionLimb.LeftIndexTip);

            var rightThumb = GestureSystem.GetLimb(InteractionLimb.RightThumbTip);
            var rightIndex = GestureSystem.GetLimb(InteractionLimb.RightIndexTip);

            if (leftThumb == null || leftIndex == null || rightThumb == null || rightIndex == null)
            {
                return IsGestureActive;
            }

            var leftStatus = CheckStatus(leftThumb, leftIndex);
            var rightStatus = CheckStatus(rightThumb, rightIndex);

            if (leftStatus == GestureStatus.Starting && rightStatus == GestureStatus.Starting)
            {
                IsGestureActive = true;
                // TODO
                _activePosition = Vector3.zero;
                RaiseGestureStartEvent();
            }
            else if (leftStatus == GestureStatus.Active && rightStatus == GestureStatus.Active)
            {
                // TODO
                _activePosition = Vector3.zero;
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
            var indexType = (TriggerHand == Hand.Left) ? InteractionLimb.LeftIndexTip : InteractionLimb.RightIndexTip;
            var thumbType = (TriggerHand == Hand.Left) ? InteractionLimb.LeftThumbTip : InteractionLimb.RightThumbTip;

            var index = GestureSystem.GetLimb(indexType);
            var thumb = GestureSystem.GetLimb(thumbType);

            if (index == null || thumb == null)
            {
                return IsGestureActive;
            }

            var status = CheckStatus(thumb, index);

            switch (status)
            {
                case GestureStatus.Starting:
                    _activePosition = index.transform.position;
                    IsGestureActive = true;
                    RaiseGestureStartEvent();
                    break;

                case GestureStatus.Active:
                    _activePosition = index.transform.position;
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
