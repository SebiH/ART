//using UnityEngine;
//using System.Collections;
//using Assets.Scripts.GestureControl;
//using System;
//using Leap;
//using System.Linq;

namespace Assets.Scripts.Gestures
{
    public class GrabGesture //: GestureBase, IPositionGesture
    {
        //public GestureControl.Hand TriggerHand;

        //private Controller controller;
        //private Vector3 grabPosition;

        //private bool IsGestureActive;

        //public override string GetName()
        //{
        //    return "GrabGesture";
        //}

        //public Vector3 GetGesturePosition(GestureControl.Hand hand)
        //{
        //    // TODO
        //    return grabPosition;
        //}

        //protected override void Start()
        //{
        //    base.Start();
        //    controller = new Controller();
        //}

        //public override bool CheckConditions()
        //{
        //    Frame frame = controller.Frame();
        //    var hands = frame.Hands;
        //    Leap.Hand currentHand = null;

        //    if (TriggerHand == GestureControl.Hand.Left)
        //    {
        //        currentHand = hands.FirstOrDefault(h => h.IsLeft);
        //    }
        //    else if (TriggerHand == GestureControl.Hand.Right)
        //    {
        //        currentHand = hands.FirstOrDefault(h => h.IsRight);
        //    }
        //    else if (TriggerHand == GestureControl.Hand.Either)
        //    {
        //        currentHand = hands.FirstOrDefault();
        //    }

        //    if (currentHand != null)
        //    {
        //        if (currentHand.GrabStrength >= 0.99)
        //        {
        //            // leapmotion's position seems to be different than our own position
        //            if (currentHand.IsLeft)
        //            {
        //                grabPosition = GestureSystem.GetLimb(InteractionLimb.LeftPalm).transform.position;
        //            }
        //            else
        //            {
        //                grabPosition = GestureSystem.GetLimb(InteractionLimb.RightPalm).transform.position;
        //            }

        //            if (IsGestureActive)
        //            {
        //                RaiseGestureActiveEvent();
        //            }
        //            else
        //            {
        //                IsGestureActive = true;
        //                RaiseGestureStartEvent();
        //            }
        //        }
        //        else if (IsGestureActive)
        //        {
        //            IsGestureActive = false;
        //            RaiseGestureStopEvent();
        //        }
        //    }
        //    else
        //    {
        //        // currenthand not found, keep event as it is
        //    }

        //    return IsGestureActive;
        //}

    }
}
