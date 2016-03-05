using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;

public class PickUp : MonoBehaviour
{
    private Vector3 offset;
    private GameObject pickedUpObject;
    private List<GameObject> ObjectsWithinReach = new List<GameObject>();

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Pickup")
        {
            ObjectsWithinReach.Add(other.gameObject);
        }
    }


    protected void OnTriggerExit(Collider other)
    {
        ObjectsWithinReach.Remove(other.gameObject);
    }


    public GameObject GetNearestPickup()
    {
        float minDistance = float.PositiveInfinity;
        GameObject nearestObject = null;

        foreach (var obj in ObjectsWithinReach)
        {
            if ((obj.transform.position - transform.position).sqrMagnitude < minDistance)
            {
                nearestObject = obj;
            }
        }

        return nearestObject;
    }


    public void StartPickup(GestureEventArgs e)
    {
        pickedUpObject = GetNearestPickup();

        if (pickedUpObject != null)
        {
            offset = pickedUpObject.transform.position - transform.position;
            pickedUpObject.GetComponent<Rigidbody>().detectCollisions = false;
        }
    }

    public void UpdatePosition(GestureEventArgs e)
    {
        if (pickedUpObject != null)
        {
            pickedUpObject.transform.position = transform.position + offset;
        }
    }

    public void EndPickup(GestureEventArgs e)
    {
        if (pickedUpObject != null)
        {
            pickedUpObject.transform.position = transform.position + offset;
            pickedUpObject.GetComponent<Rigidbody>().detectCollisions = true;
            pickedUpObject = null;
        }
    }
}
