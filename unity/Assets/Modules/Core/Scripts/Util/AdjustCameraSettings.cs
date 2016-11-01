using UnityEngine;
using System.Collections;

/**
 *  Adjusts a few camera settings which are overridden by SteamVR (e.g. FoV)
 */
public class AdjustCameraSettings : MonoBehaviour
{
    [Range(60, 120)]
    public int TargetFieldOfView;

    void Update()
    {
        GetComponent<Camera>().fieldOfView = TargetFieldOfView;
        //Destroy(GetComponent<AdjustCameraSettings>());
	}
}
