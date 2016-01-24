using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Base class for all triggers
/// </summary>
public abstract class BaseTrigger : MonoBehaviour
{
    public UnityEvent<Vector3> OnGestureDetected;
    public UnityEvent<Vector3> OnGestureHold;
    public UnityEvent<Vector3> OnGestureStop;

    protected void FireGestureDetected(Vector3 pos)
    {
        if (OnGestureDetected != null)
            OnGestureDetected.Invoke(pos);
    }


    protected void FireGestureHold(Vector3 pos)
    {
        if (OnGestureHold != null)
            OnGestureHold.Invoke(pos);
    }

    protected void FireGestureStop(Vector3 pos)
    {
        if (OnGestureStop != null)
            OnGestureStop.Invoke(pos);
    }
}
