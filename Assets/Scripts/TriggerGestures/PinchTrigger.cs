using System.Collections;
using UnityEngine;

public class PinchTrigger : BaseTrigger {

    public string TriggerFingerTag = "index";

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TriggerFingerTag)
        {
            FireGestureDetected(GetCollisionPosition(other));
            StopCoroutine("TimeoutGestureStop");
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
            FireGestureStop(GetCollisionPosition(other));
        }
    }


    private Vector3 GetCollisionPosition(Collider collider)
    {
        // TODO: proper position of collision?
        // [1] mentions contacts property, but collider doesn't seem to have this?
        // [1]: http://docs.unity3d.com/ScriptReference/Collision.html
        return transform.position;
    }

}
