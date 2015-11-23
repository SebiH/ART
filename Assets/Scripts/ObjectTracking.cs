using UnityEngine;
using Assets;

public class ObjectTracking : MonoBehaviour {

    private Vector3 cameraPosition;
    private int currentTransformId;

	void Start ()
    {
        // remember starting position as camera location
        cameraPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update ()
    {
        var newTransformId = CameraImageProvider.GetImageGeneration();

        if (newTransformId > currentTransformId)
        {
            var pose = CameraImageProvider.GetCurrentPose();

            var newPos = new Vector3((float)pose[0]/100, -(float)pose[2]/100, -(float)pose[1]/100);
            var oldPos = cameraPosition;

            transform.position = newPos - oldPos;
            //transform.rotation = Quaternion.Euler((float) pose[3], (float) pose[4], (float) pose[5]);

            newTransformId = currentTransformId;
        }
	}
}
