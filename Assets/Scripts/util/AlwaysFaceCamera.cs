using UnityEngine;
using System.Collections;

public class AlwaysFaceCamera : MonoBehaviour
{
    public GameObject Camera;

	void Update ()
    {
        transform.LookAt(Camera.transform);
	}
}
