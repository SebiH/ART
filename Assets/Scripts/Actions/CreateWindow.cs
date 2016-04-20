using UnityEngine;
using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;

public class CreateWindow : MonoBehaviour
{
    public int MinWidth = 50;
    public int MinHeight = 50;

    public GameObject Template;
    private GameObject _createdWindow;

    public void StartCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;
        
        if (gesture != null)
        {
            _createdWindow = Instantiate(Template);

            _createdWindow.transform.position = gesture.GetGesturePosition(Hand.Both);
            _createdWindow.transform.localScale = GetScale(gesture);
            _createdWindow.transform.rotation = Quaternion.Euler(GetRotation(gesture));
        }
    }

    public void UpdateCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null && _createdWindow != null)
        {
            _createdWindow.transform.position = gesture.GetGesturePosition(Hand.Both);
            _createdWindow.transform.localScale = GetScale(gesture);
            _createdWindow.transform.rotation = Quaternion.Euler(GetRotation(gesture));
        }
    }

    public void FinishCreation(GestureEventArgs e)
    {
        _createdWindow = null;
    }


    private Vector3 GetScale(IPositionGesture gesture)
    {
        var gesturePosLeft = gesture.GetGesturePosition(Hand.Left);
        var gesturePosRight = gesture.GetGesturePosition(Hand.Right);

        var MinScale = 0.001f;
        var MaxScale = 1;
        var scale = gesturePosLeft - gesturePosRight;
        var scaleX = Mathf.Clamp(Mathf.Abs(scale.x), MinScale, MaxScale) / 10f;
        var scaleY = 1;
        var scaleZ = Mathf.Clamp(Mathf.Abs(scale.y), MinScale, MaxScale) / 10f;

        return new Vector3(scaleX, scaleY, scaleZ);
    }

    private Vector3 GetRotation(IPositionGesture gesture)
    {
        var gesturePosLeft = gesture.GetGesturePosition(Hand.Left);
        var gesturePosRight = gesture.GetGesturePosition(Hand.Right);

        var delta = gesturePosRight - gesturePosLeft;
        var rad = Mathf.Atan2(delta.z, delta.x);
        var deg = rad * (180f / Mathf.PI);

        if (deg < -90)
            deg += 180;
        if (deg > 90)
            deg -= 180;

        return new Vector3(270, -deg, 0);
    }
}
