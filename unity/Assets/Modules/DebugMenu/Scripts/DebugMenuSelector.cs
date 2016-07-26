using UnityEngine;
using System.Collections;

public class DebugMenuSelector : MonoBehaviour
{
    void FixedUpdate()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, fwd, out hit, 3f))
        {
            var hitObject = hit.transform.gameObject;
            var menuEntry = hitObject.GetComponent<DebugMenuEntry>();

            if (menuEntry != null)
            {
                var isMouseDown = Input.GetMouseButtonDown(0);
                var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
                var isTriggerDown = (deviceIndex != -1) && SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);

                if (isMouseDown || isTriggerDown)
                {
                    DebugMenu.Instance.SelectMenuEntry(hitObject);
                }
                else
                {
                    DebugMenu.Instance.FocusMenuEntry(hitObject);
                }
            }
        }
        else
        {
            DebugMenu.Instance.UnfocusAll();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + (transform.TransformDirection(Vector3.forward) * 0.3f));
    }
}
