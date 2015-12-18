using UnityEngine;

public class DataPoint : MonoBehaviour
{

	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}


    public void SetHeight(float height)
    {
        // TODO: animation?
        var currPos = transform.localPosition;
        transform.localPosition = new Vector3(currPos.x, height / 2, currPos.z);
        transform.localScale = new Vector3(1, height, 1);
    }

    public void SetPosition(float x, float y)
    {
        var currPos = transform.localPosition;
        transform.localPosition = new Vector3(x, currPos.y, y);
    }
}
