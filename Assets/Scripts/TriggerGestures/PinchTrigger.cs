using UnityEngine;
using System.Collections;

public class PinchTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "index")
        {
            print("On Trigger entered");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "index")
        {
            print("On Trigger exit");
        }
    }
}
