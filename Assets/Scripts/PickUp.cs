using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour
{
    private SpringJoint joint;

    public void StartPickup()
    {
        var finger = GameObject.FindGameObjectsWithTag("thumb")[0];
        joint = finger.gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();
        joint.spring = 60f;
        joint.damper = 0.2f;
        joint.maxDistance = 0f;
        joint.minDistance = 0f;
    }

    public void EndPickup()
    {
        Destroy(joint);
    }
}
