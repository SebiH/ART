using UnityEngine;
using UnityEngine.VR;

public class ResetOculusPosition : MonoBehaviour {

    public KeyCode ResetKey = KeyCode.Space;

	void Update ()
    {
	    if (Input.GetKeyDown(ResetKey))
        {
            InputTracking.Recenter();
        }
	}
}
