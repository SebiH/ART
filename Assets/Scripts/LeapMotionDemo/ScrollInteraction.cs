/* Has to be attached to a Collision Trigger Object! */

using UnityEngine;
using System.Collections;
using Assets.Code.Logging;
using UnityEngine.UI;

public class ScrollInteraction : MonoBehaviour
{
    public ScrollRect scrollElement;

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
        // TODO: better alternative?
        if (collider == scrollCollider)
        {
            var movement = collider.transform.position.y - prevColliderPos;
            scrollElement.verticalNormalizedPosition -= movement / 100f;
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
