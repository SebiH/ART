using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;
using UnityEngine;

public class CreateObject : MonoBehaviour
{
    public GameObject Template;

    public float MinScale = 0.05f;
    public float MaxScale = 0.25f;

    private GameObject CreatedInstance;

    public void StartCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            CreatedInstance = Instantiate(Template);
            CreatedInstance.transform.position = gesture.GetGesturePosition(Hand.Both);

            var scale = GetScale(gesture);
            CreatedInstance.transform.localScale = new Vector3(scale, scale, scale);

            // turn off rigidbody during creation
            var rigidbody = CreatedInstance.GetComponent<Rigidbody>();
            rigidbody.detectCollisions = false;
            rigidbody.mass = scale;
        }
    }

    public void ModifyCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            CreatedInstance.transform.position = gesture.GetGesturePosition(Hand.Both);

            var scale = GetScale(gesture);
            CreatedInstance.transform.localScale = new Vector3(scale, scale, scale);

            var rigidbody = CreatedInstance.GetComponent<Rigidbody>();
            rigidbody.mass = scale;
        }
    }


    public void FinishCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            // turn on rigidbody on again
            CreatedInstance.GetComponent<Rigidbody>().detectCollisions = true;
            CreatedInstance.transform.position = gesture.GetGesturePosition(Hand.Both);

            CreatedInstance = null;
        }
    }


    private float GetScale(IPositionGesture gesture)
    {
        var gesturePosLeft = gesture.GetGesturePosition(Hand.Left);
        var gesturePosRight = gesture.GetGesturePosition(Hand.Right);

        var scale = (gesturePosLeft - gesturePosRight).magnitude;

        // reduce scale a bit to avoid operlapping with fingers
        scale -= 0.025f;

        return Mathf.Clamp(scale, MinScale, MaxScale);
    }

}
