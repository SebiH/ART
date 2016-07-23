/**
 * Adapted from johny3212
 * Written by Matt Oskamp
 */
using UnityEngine;

public class OptiTrackObject : MonoBehaviour
{

    public int rigidbodyIndex;
    public Vector3 rotationOffset;

    public bool TrackPosition = true;
    public bool TrackRotation = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (TrackPosition)
        {
            Vector3 pos = OptiTrackManager.Instance.getPosition(rigidbodyIndex);
            transform.position = pos;
        }

        if (TrackRotation)
        {
            Quaternion rot = OptiTrackManager.Instance.getOrientation(rigidbodyIndex);
            rot = rot * Quaternion.Euler(rotationOffset);
            transform.rotation = rot;
        }
    }
}
