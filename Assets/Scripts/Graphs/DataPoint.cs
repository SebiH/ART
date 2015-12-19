using UnityEngine;

public class DataPoint : MonoBehaviour
{
    private bool _isHighlighted;
    public bool IsHighlighted
    {
        get { return _isHighlighted; }
        set
        {
            _isHighlighted = value;
            OnHighlightChange(value);
        }
    }



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


    private void OnHighlightChange(bool isHighlighted)
    {
        var renderer = GetComponent<MeshRenderer>();
        var color = isHighlighted ? Color.yellow : Color.white;
        renderer.material.color = color;
    }
}
