using Assets.Scripts.GestureControl;
using UnityEngine;

namespace Assets.Scripts.Gestures
{
    interface IPositionGesture
    {
        Vector3 GetGesturePosition(Hand hand);
    }
}
