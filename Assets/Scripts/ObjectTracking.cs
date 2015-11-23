using UnityEngine;
using Assets;
using System;

public class ObjectTracking : MonoBehaviour {

    private int currentTransformId;

    public Transform prefab;

	void Start ()
    {

        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {

                var height = UnityEngine.Random.value * 10;

                var obj = Instantiate(prefab);
                obj.transform.parent = transform;

                obj.transform.localScale = new Vector3(0.05f, height, 0.05f);

                obj.transform.position = Vector3.zero;
                obj.transform.localPosition = new Vector3(0.5f - (i/10f) * 0.5f, height/2, 0.5f - (j/10f) * 0.5f);

                obj.transform.localRotation = new Quaternion();
                obj.transform.rotation = new Quaternion();
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
        var newTransformId = CameraImageProvider.GetImageGeneration();

        if (newTransformId > currentTransformId)
        {
            var pose = CameraImageProvider.GetCurrentPose();

            if (Math.Abs(pose[0]) > 0.001)
            {
                transform.localPosition = new Vector3((float)pose[0] / 100, -(float)pose[2] / 100, (float)pose[1] / 100);
                //transform.localRotation = Quaternion.Euler((float)pose[3], (float)pose[4], (float)pose[5]);
            }

            newTransformId = currentTransformId;
        }
	}
}
