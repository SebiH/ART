/* Has to be attached to a Collision Trigger Object! */

using UnityEngine;

public class ScrollInteraction : MonoBehaviour
{
    public GameObject scrollElement;
    public float ForceMultiplier = 100;

    // only scroll on the first collider that entered
    // TODO: better alternative?
    private Collider scrollCollider;
    private float prevColliderPos;

    private void OnTriggerEnter(Collider collider)
    {
        if (scrollCollider == null)
        {
            scrollCollider = collider;
            prevColliderPos = collider.transform.position.y;
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        // only allow interaction on the first collider that entered the trigger
        if (collider == scrollCollider)
        {
            var movement = collider.transform.position.y - prevColliderPos;
            scrollElement.GetComponent<Rigidbody>().AddForce(new Vector3(0, movement * ForceMultiplier, 0));
            prevColliderPos = collider.transform.position.y;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider == scrollCollider)
        {
            scrollCollider = null;
        }
    }
}
