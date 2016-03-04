using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugGestures : MonoBehaviour
{
    private List<Vector3> GestureStartPoints = new List<Vector3>();
    private List<Vector3> GestureActivePoints = new List<Vector3>();
    private List<Vector3> GestureEndPoints = new List<Vector3>();

    public void OnGestureStart(GestureEventArgs e)
    {
        Debug.Log(String.Format("Gesture Start: {0}", e.Sender.GetName()));

        var gesture = e.Sender as DoublePinchDragGesture;

        if (gesture != null)
        {
            GestureStartPoints.Clear();
            GestureEndPoints.Clear();

            GestureStartPoints.Add(gesture.GetGesturePosition(Hand.Left));
            GestureStartPoints.Add(gesture.GetGesturePosition(Hand.Right));
            GestureStartPoints.Add(gesture.GetGesturePosition(Hand.Both));
        }
    }

    public void OnGestureActive(GestureEventArgs e)
    {
        var gesture = e.Sender as DoublePinchDragGesture;

        if (gesture != null)
        {
            GestureActivePoints.Add(gesture.GetGesturePosition(Hand.Left));
            GestureActivePoints.Add(gesture.GetGesturePosition(Hand.Right));
            GestureActivePoints.Add(gesture.GetGesturePosition(Hand.Both));
        }
    }

    public void OnGestureStop(GestureEventArgs e)
    {
        Debug.Log(String.Format("Gesture Stop: {0}", e.Sender.GetName()));

        var gesture = e.Sender as DoublePinchDragGesture;

        if (gesture != null)
        {
            GestureActivePoints.Clear();

            GestureEndPoints.Add(gesture.GetGesturePosition(Hand.Left));
            GestureEndPoints.Add(gesture.GetGesturePosition(Hand.Right));
            GestureEndPoints.Add(gesture.GetGesturePosition(Hand.Both));
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var point in GestureStartPoints)
        {
            Gizmos.DrawSphere(point, 0.01f);
        }

        Gizmos.color = Color.yellow;
        // only draw a part of the active gestures
        while (GestureActivePoints.Count > 21)
        {
            GestureActivePoints.RemoveAt(0);
        }

        foreach (var point in GestureActivePoints)
        {
            Gizmos.DrawSphere(point, 0.01f);
        }

        Gizmos.color = Color.red;
        foreach (var point in GestureEndPoints)
        {
            Gizmos.DrawSphere(point, 0.01f);
        }
    }
}
