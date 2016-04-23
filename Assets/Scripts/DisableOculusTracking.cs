using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class DisableOculusTracking : MonoBehaviour
{
    [DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ovrp_SetCaps(int caps);

    void Start()
    {
        // disable position tracking
        ovrp_SetCaps(14);
        // disable rotation tracking
        ovrp_SetCaps(6);
    }
}
