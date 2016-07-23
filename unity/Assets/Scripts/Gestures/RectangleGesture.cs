using Assets.Scripts.GestureControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RectangleGesture : GestureBase
{
    private bool IsGestureActive;

    public override string GetName()
    {
        return "RectangleGesture";
    }

    public override bool CheckConditions()
    {
        var limbs = GestureSystem.GetLimbs(new[]
        {
            InteractionLimb.LeftThumbTip,
            InteractionLimb.LeftThumbBase,
            InteractionLimb.LeftIndexTip,
            InteractionLimb.LeftIndexBase,

            InteractionLimb.RightThumbTip,
            InteractionLimb.RightThumbBase,
            InteractionLimb.RightIndexTip,
            InteractionLimb.RightIndexBase
        });

        if (limbs.Values.Any((l) => l == null)) 
        {
            return IsGestureActive;
        }

        var status = CheckStatus(limbs);

        if (status == GestureStatus.Starting)
        {
            IsGestureActive = true;
            RaiseGestureStartEvent();
        }
        else if (status == GestureStatus.Active)
        {
            RaiseGestureActiveEvent();
        }
        else if (status == GestureStatus.Stopping)
        {
            IsGestureActive = false;
            RaiseGestureStopEvent();
        }

        return IsGestureActive;
    }


    private GestureStatus CheckStatus(Dictionary<InteractionLimb, GameObject> limbs)
    {
        // TODO.
        return GestureStatus.Inactive;
    }
}
