using Assets.Scripts.GestureControl;
using System;
using UnityEngine;

public class DebugGestures : MonoBehaviour
{
    public void OnGestureStart(GestureEventArgs e)
    {
        Debug.Log(String.Format("Gesture Start: {0}", e.Sender.GetName()));
    }

    public void OnGestureActive(GestureEventArgs e)
    {

    }

    public void OnGestureStop(GestureEventArgs e)
    {
        Debug.Log(String.Format("Gesture Stop: {0}", e.Sender.GetName()));
    }
}
