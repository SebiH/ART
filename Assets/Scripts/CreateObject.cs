using UnityEngine;
using System.Collections;
using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;

public class CreateObject : MonoBehaviour
{
    public GameObject Template;

    private GameObject CreatedInstance;

    public void StartCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            CreatedInstance = Instantiate(Template);
            CreatedInstance.transform.position = gesture.GetGesturePosition(Hand.Both);
            CreatedInstance.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            // turn off rigidbody during creation
            var rigidbody = CreatedInstance.GetComponent<Rigidbody>();
            rigidbody.detectCollisions = false;
            rigidbody.mass *= 0.01f; // TODO: scale this proportional to the object's size
        }
    }


    public void ModifyCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            CreatedInstance.transform.position = gesture.GetGesturePosition(Hand.Both);
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

}
