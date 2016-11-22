using UnityEngine;
using Assets.Modules.Tracking;

public class HitboxTest : MonoBehaviour
{
    public string DisplayName = "Surface";

    public Transform MoveableObject;

	void Update ()
    {
        if (FixedDisplays.Has(DisplayName))
        {
            var display = FixedDisplays.Get(DisplayName);
            transform.localScale = display.Scale;
            // TODO: preserve relative scale of moveable object?
            MoveableObject.localScale = new Vector3(MoveableObject.localScale.x / transform.localScale.x, MoveableObject.localScale.y / transform.localScale.y, MoveableObject.localScale.z / transform.localScale.z);
            transform.position = display.Position;
            transform.rotation = display.Rotation;

            // job done
            enabled = false;
        }
	}
}
