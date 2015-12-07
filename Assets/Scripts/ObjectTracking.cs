using Assets.Code;
using System;
using UnityEngine;

public class ObjectTracking : MonoBehaviour {

    private int currentTransformId;

    public Transform prefab;
    public float scaling = 200;

	void Start ()
    {
        tag = "tracked";
    }

    // Update is called once per frame
    void Update ()
    {
        var newTransformId = CameraImageProvider.GetImageGeneration();

        if (newTransformId > currentTransformId)
        {
            var pose = CameraImageProvider.GetCurrentPose();

            // TODO: add parameter to pose to indicate if a pose was found
            if (Math.Abs(pose.translationX) > 0.001)
            {
                transform.localPosition = new Vector3((float)pose.translationX / scaling, -(float)pose.translationY / scaling, (float)pose.translationZ / scaling);

                float a = (float)pose.rotationX;
                float b = (float)pose.rotationY;
                float c = (float)pose.rotationZ;

                transform.localRotation = Quaternion.Euler(a / Mathf.PI * 180f, c / Mathf.PI * 180f, b / Mathf.PI * 180f);
            }

            currentTransformId = newTransformId;
        }
	}
}
