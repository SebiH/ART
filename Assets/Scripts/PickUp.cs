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
            var body = pickedUpObject.GetComponent<Rigidbody>();

            offset = pickedUpObject.transform.position - transform.position;
            lastGesturePositions.Add(pickedUpObject.transform.position);

            body.detectCollisions = false;
            // stop all motion
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.Sleep();
        }
    }

    private List<Vector3> lastGesturePositions = new List<Vector3>();

    public void UpdatePosition(GestureEventArgs e)
    {
        if (pickedUpObject != null)
        {
            pickedUpObject.transform.position = transform.position + offset;

            lastGesturePositions.Add(pickedUpObject.transform.position);

            while (lastGesturePositions.Count > 5)
            {
                lastGesturePositions.RemoveAt(0);
            }
        }
    }

    public void EndPickup(GestureEventArgs e)
    {
        if (pickedUpObject != null)
        {
            pickedUpObject.transform.position = transform.position + offset;

            var body = pickedUpObject.GetComponent<Rigidbody>();
            body.detectCollisions = true;

            // apply last known velocity of gesture
            var velocity = pickedUpObject.transform.position - GestureUtil.GetCenterPosition(lastGesturePositions);
            body.AddForce(velocity * 60);
            lastGesturePositions.Clear();

            pickedUpObject = null;
        }
    }
}
