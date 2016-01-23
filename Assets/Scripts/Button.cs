using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour
{
    private bool isPressed;
    public GameObject ButtonObj;

    public float ClickedPositionChange = -0.1f;
    public Color ClickedColour = Color.green;

    private Color OriginalColour;

    private void Start()
    {
        OriginalColour = ButtonObj.GetComponent<Renderer>().material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: tagmanager -> hasTag ( canTriggerInteraction ) ?
        if (!isPressed)
        {
            isPressed = true;

            // move button down to indicate pressed state
            StopCoroutine("AnimatePosition");
            StartCoroutine("AnimatePosition", true);

            // change colour
            StopCoroutine("ChangeColour");
            StartCoroutine("ChangeColour", ClickedColour);
        }

    }


    private void OnTriggerExit(Collider other)
    {
        // TODO: tagmanager -> hasTag ( canTriggerInteraction ) ?

        if (isPressed)
        {
            isPressed = false;

            // go to default location/state/colour
            StopCoroutine("AnimatePosition");
            StartCoroutine("AnimatePosition", false);

            StopCoroutine("ChangeColour");
            StartCoroutine("ChangeColour", Color.white);
        }
    }


    private IEnumerator AnimatePosition(bool isPressed)
    {
        var currentVelocity = 0f;
        var targetHeight = (isPressed) ? ClickedPositionChange : 0f;
        var currentHeight = ButtonObj.transform.localPosition.y; 

        while (Mathf.Abs(currentHeight - targetHeight) > Mathf.Epsilon)
        {
            var newHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref currentVelocity, 0.05f);

            currentHeight = ButtonObj.transform.localPosition.y;
            ButtonObj.transform.localPosition = new Vector3(ButtonObj.transform.localPosition.x, newHeight, ButtonObj.transform.localPosition.z);
            // resume after next update
            yield return null;
        }
    }

    private IEnumerator ChangeColour(Color to)
    {
        var renderer = ButtonObj.GetComponent<MeshRenderer>();
        var colorDistance = renderer.material.color - to;

        while (Mathf.Abs(colorDistance.r) > Mathf.Epsilon ||
               Mathf.Abs(colorDistance.g) > Mathf.Epsilon ||
               Mathf.Abs(colorDistance.b) > Mathf.Epsilon ||
               Mathf.Abs(colorDistance.a) > Mathf.Epsilon)
        {
            var newColour = Color.Lerp(renderer.material.color, to, Time.deltaTime * 10f);
            renderer.material.color = newColour;

            // resume after next update
            yield return null;
        }

        renderer.material.color = to;
    }
}
