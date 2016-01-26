using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour
{
    private FixedJoint joint;

    public void StartPickup()
    {
        var finger = GameObject.FindGameObjectsWithTag("index")[0];
        joint = finger.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();
    }

    public void EndPickup()
    {
        Destroy(joint);
    }
}
