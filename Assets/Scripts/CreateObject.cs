using Assets.Scripts.GestureControl;
using Assets.Scripts.Gestures;
using System.Collections.Generic;
using UnityEngine;

public class CreateObject : MonoBehaviour
{
    public GameObject Template;

    public float MinScale = 0.05f;
    public float MaxScale = 0.25f;

    // creates cubes, if on; or rectangles if off
    public bool UseUniformScale = true;

    private GameObject CreatedInstance;
    private List<GameObject> _createdInstances = new List<GameObject>();

    public void StartCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            CreatedInstance = Instantiate(Template);
            CreatedInstance.transform.position = gesture.GetGesturePosition(Hand.Both);

            var scale = GetScale(gesture);
            CreatedInstance.transform.localScale = scale;

            // turn off rigidbody during creation
            var rigidbody = CreatedInstance.GetComponent<Rigidbody>();
            rigidbody.detectCollisions = false;
            rigidbody.mass = GetMass(scale);
        }
    }

    private List<Vector3> lastGesturePositions = new List<Vector3>();

    public void ModifyCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            var pos = gesture.GetGesturePosition(Hand.Both);
            CreatedInstance.transform.position = pos;
            lastGesturePositions.Add(pos);

            while (lastGesturePositions.Count > 10)
            {
                lastGesturePositions.RemoveAt(0);
            }

            var scale = GetScale(gesture);
            CreatedInstance.transform.localScale = scale;

            var rigidbody = CreatedInstance.GetComponent<Rigidbody>();
            rigidbody.mass = GetMass(scale);
        }
    }


    public void FinishCreation(GestureEventArgs e)
    {
        var gesture = e.Sender as IPositionGesture;

        if (gesture != null)
        {
            // turn on rigidbody on again
            var body = CreatedInstance.GetComponent<Rigidbody>();
            body.detectCollisions = true;
            var pos = gesture.GetGesturePosition(Hand.Both);
            CreatedInstance.transform.position = pos;

            // apply last known velocity of gesture
            var velocity = pos - GestureUtil.GetCenterPosition(lastGesturePositions);
            body.AddForce(velocity * 50);
            lastGesturePositions.Clear();

            _createdInstances.Add(CreatedInstance);
            CreatedInstance = null;
        }
    }


    public void ApplyUpForce(float force)
    {
        ApplyForceToCreatedObjects(new Vector3(0, force, 0));
    }


    public void ApplyForwardForce(float force)
    {
        ApplyForceToCreatedObjects(new Vector3(0, 0, force));
    }

    public void ApplySideForce(float force)
    {
        ApplyForceToCreatedObjects(new Vector3(force, 0, 0));
    }

    private void ApplyForceToCreatedObjects(Vector3 force)
    {
        foreach (var instance in _createdInstances)
        {
            var body = instance.GetComponent<Rigidbody>();
            body.AddForce(force);
        }
    }

    private Vector3 GetScale(IPositionGesture gesture)
    {
        var gesturePosLeft = gesture.GetGesturePosition(Hand.Left);
        var gesturePosRight = gesture.GetGesturePosition(Hand.Right);


        if (UseUniformScale)
        {
            var scale = (gesturePosLeft - gesturePosRight).magnitude;

            // reduce scale a bit to avoid operlapping with fingers
            scale -= 0.025f;

            scale = Mathf.Clamp(scale, MinScale, MaxScale);
            return new Vector3(scale, scale, scale);
        }
        else
        {
            var scale = gesturePosLeft - gesturePosRight;
            var scaleX = Mathf.Clamp(Mathf.Abs(scale.x) - 0.025f, MinScale, MaxScale);
            var scaleY = Mathf.Clamp(Mathf.Abs(scale.y), MinScale, MaxScale);
            var scaleZ = Mathf.Clamp(Mathf.Abs(scale.z), MinScale, MaxScale);

            return new Vector3(scaleX, scaleY, scaleZ);
        }
    }

    private float GetMass(Vector3 volume)
    {
        return (volume.x + volume.y + volume.z) / 15f;
    }

}
