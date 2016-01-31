using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour
{
    private FixedJoint joint;
    public GameObject attachTo;

    public void StartPickup()
    {
        transform.position = attachTo.transform.position;
        joint = attachTo.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();
    }

    public void EndPickup()
    {
        Destroy(joint);
    }
}
