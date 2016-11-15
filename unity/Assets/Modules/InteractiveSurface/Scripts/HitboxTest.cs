using UnityEngine;
using Assets.Modules.Tracking;

public class HitboxTest : MonoBehaviour
{
    public string DisplayName = "Surface";

	void Update ()
    {
        if (FixedDisplays.Has(DisplayName))
        {
            var display = FixedDisplays.Get(DisplayName);
            transform.localScale = display.Scale;
            transform.position = display.Position;
            transform.rotation = display.Rotation;

            // job done
            enabled = false;
        }
	}
}
