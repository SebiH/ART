using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Base class for all triggers
/// </summary>
public abstract class BaseTrigger : MonoBehaviour
{
    public UnityEvent<Vector3> OnGestureDetected;
    public UnityEvent<Vector3> OnGestureHold;
    public UnityEvent<Vector3> OnGestureStop;

    public bool DrawDebugPoints = false;

    protected void FireGestureDetected(Vector3 pos)
    {
        if (OnGestureDetected != null)
            OnGestureDetected.Invoke(pos);
    }


    protected void FireGestureHold(Vector3 pos)
    {
        if (OnGestureHold != null)
            OnGestureHold.Invoke(pos);


        if (DrawDebugPoints)
        {
            while (detectedPositions.Count >= 20)
            {
                detectedPositions.Dequeue();
            }

            detectedPositions.Enqueue(pos);
        }
    }

    protected void FireGestureStop(Vector3 pos)
    {
        if (OnGestureStop != null)
            OnGestureStop.Invoke(pos);

        if (DrawDebugPoints)
        {
            detectedPositions.Clear();
        }
    }


    

    private Queue<Vector3> detectedPositions = new Queue<Vector3>(20);
    private void OnDrawGizmos()
    {
        if (DrawDebugPoints)
        {
            Gizmos.color = Color.blue;

            foreach (var pos in detectedPositions)
            {
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }
}
