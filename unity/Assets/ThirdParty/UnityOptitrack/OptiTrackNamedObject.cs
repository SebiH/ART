/**
 * Adapted from johny3212
 * Written by Matt Oskamp
 */
using UnityEngine;

public class OptiTrackNamedObject : MonoBehaviour
{

    public string rigidbodyName;
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
            Vector3 pos = OptiTrackManager.Instance.getPosition(rigidbodyName);
            transform.position = pos;
        }

        if (TrackRotation)
        {
            Quaternion rot = OptiTrackManager.Instance.getOrientation(rigidbodyName);
            rot = rot * Quaternion.Euler(rotationOffset);
            transform.rotation = rot;
        }
    }
}
