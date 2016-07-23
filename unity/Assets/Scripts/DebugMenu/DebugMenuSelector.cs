using UnityEngine;
using System.Collections;

public class DebugMenuSelector : MonoBehaviour
{
    void FixedUpdate()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, fwd, out hit, 1f))
        {
            var hitObject = hit.transform.gameObject;
            var menuEntry = hitObject.GetComponent<DebugMenuEntry>();

            if (menuEntry != null)
            {
                if (Input.GetMouseButtonDown(0))
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
