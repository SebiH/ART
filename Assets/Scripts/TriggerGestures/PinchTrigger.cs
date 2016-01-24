using UnityEngine;
using System.Collections;

public class PinchTrigger : MonoBehaviour {

    public string TriggerFingerTag = "index";

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TriggerFingerTag)
        {
            print("On Trigger enter");
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.tag == TriggerFingerTag)
        {
            print("On Trigger stay");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == TriggerFingerTag)
        {
            print("On Trigger exit");
        }
    }
}
