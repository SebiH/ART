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

    private float _height;
    public float Height
    {
        get { return _height; }
        set
        {
            var currPos = transform.localPosition;
            transform.localPosition = new Vector3(currPos.x, value / 2, currPos.z);
            transform.localScale = new Vector3(1, value, 1);
            _height = value;
        }
    }


    private Color _color;
    public Color Color
    {
        get { return _color;  }
        set
        {
            var renderer = GetComponent<MeshRenderer>();
            renderer.material.color = value;
            _color = value;
        }
    }

    void Start ()
    {
	}
	
	void Update ()
    {
        if (isAnimating)
        {
            var interpolatedHeight = Mathf.Lerp(Height, destinationHeight, animationTime);
            animationTime += Time.deltaTime;
            Height = interpolatedHeight;

            // stop animating once we have reached desired height
            if (Mathf.Abs(destinationHeight - Height) < Mathf.Epsilon)
            {
                isAnimating = false;
            }
        }
	}


    private float destinationHeight;
    private float animationTime;
    private bool isAnimating;
    public void SetHeight(float height)
    {
        isAnimating = true;
        destinationHeight = height;
        animationTime = 0;
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
