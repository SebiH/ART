using UnityEngine;
using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;

public class DataManipulationTest : MonoBehaviour
{
    private bool _gestureActive = false;
    private Vector3 _gesturePrevPos;
    public GameObject ScaleObject;

    public void OnGestureStart(GestureEventArgs e)
    {
        var originGesture = e.Sender as IPositionGesture;
        var gesturePos = originGesture.GetGesturePosition(Hand.Right);

        if ((gesturePos - (transform.position + new Vector3(0, transform.localScale.y, 0))).magnitude < 0.1f)
        {
            _gestureActive = true;
            _gesturePrevPos = gesturePos;
        }
    }

    public void OnGestureMove(GestureEventArgs e)
    {
        var originGesture = e.Sender as IPositionGesture;
        var gesturePos = originGesture.GetGesturePosition(Hand.Right);

        var difference = gesturePos.y - _gesturePrevPos.y;
        transform.localScale -= new Vector3(0, difference, 0);

        if (ScaleObject != null)
        {
            ScaleObject.transform.localScale -= new Vector3(0, difference, 0);
        }

        _gesturePrevPos = gesturePos;
    }

    public void OnGestureStop(GestureEventArgs e)
    {
        _gestureActive = false;
    }
}
