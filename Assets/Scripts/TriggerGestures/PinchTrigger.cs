using System.Collections;
using UnityEngine;

public class PinchTrigger : BaseTrigger {

    public string TriggerFingerTag = "index";
    public float GestureLostTimeout = 0.5f;

    private bool IsGestureDetected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TriggerFingerTag)
        {
            StopCoroutine("TimeoutGestureStop");

            if (!IsGestureDetected)
            {
                FireGestureDetected(GetCollisionPosition(other));
                IsGestureDetected = true;
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.tag == TriggerFingerTag)
        {
            FireGestureHold(GetCollisionPosition(other));
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == TriggerFingerTag)
        {
            StartCoroutine("TimeoutGestureStop", GetCollisionPosition(other));
        }
    }

    private IEnumerator TimeoutGestureStop(Vector3 pos)
    {
        float totalTime = 0f;

        while (totalTime < GestureLostTimeout)
        {
            totalTime += Time.deltaTime;
            yield return null;
        }

        // threshold reached
        IsGestureDetected = false;
        FireGestureStop(pos);
    }


    private Vector3 GetCollisionPosition(Collider collider)
    {
        // TODO: proper position of collision?
        // [1] mentions contacts property, but collider doesn't seem to have this?
        // [1]: http://docs.unity3d.com/ScriptReference/Collision.html
        return transform.position;
    }

}
