/* Has to be attached to a Collision Trigger Object! */

using UnityEngine;
using System.Collections;
using Assets.Code.Logging;

public class ScrollInteraction : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        Logger.Log("Trigger enter!");
    }

    private void OnTriggerStay(Collider collider)
    {
        Logger.Log("Trigger stay");
    }

    private void OnTriggerExit(Collider collider)
    {
        Logger.Log("Trigger exit");
    }
}
