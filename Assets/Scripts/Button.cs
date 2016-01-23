using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour
{
    private bool isPressed;
    public GameObject ButtonObj;

    public Vector3 ClickedPosition = new Vector3(0f, -0.1f, 0f);
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
            StartCoroutine("AnimatePosition", ClickedPosition);

            // change colour
            StopCoroutine("AnimateColour");
            StartCoroutine("AnimateColour", ClickedColour);
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
            StartCoroutine("AnimatePosition", Vector3.zero);

            StopCoroutine("AnimateColour");
            StartCoroutine("AnimateColour", OriginalColour);
        }
    }


    private IEnumerator AnimatePosition(Vector3 to)
    {
        while ((ButtonObj.transform.localPosition - to).sqrMagnitude > Mathf.Epsilon)
        {
            ButtonObj.transform.localPosition = Vector3.Lerp(ButtonObj.transform.localPosition, to, Time.deltaTime * 15f);
            // resume after next update
            yield return null;
        }
    }

    private IEnumerator AnimateColour(Color to)
    {
        var renderer = ButtonObj.GetComponent<MeshRenderer>();
        var colorDistance = renderer.material.color - to;

        while (Mathf.Abs(colorDistance.r) > Mathf.Epsilon ||
               Mathf.Abs(colorDistance.g) > Mathf.Epsilon ||
               Mathf.Abs(colorDistance.b) > Mathf.Epsilon ||
               Mathf.Abs(colorDistance.a) > Mathf.Epsilon)
        {
            var newColour = Color.Lerp(renderer.material.color, to, Time.deltaTime * 15f);
            renderer.material.color = newColour;

            // resume after next update
            yield return null;
        }

        renderer.material.color = to;
    }
}
