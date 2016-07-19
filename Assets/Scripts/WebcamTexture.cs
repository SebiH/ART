using UnityEngine;
using System.Collections;

public class WebcamTexture : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        foreach (var webcam in WebCamTexture.devices)
        {
            Debug.Log(webcam.name);
        }

        var tex = new WebCamTexture("MMP SDK");
        GetComponent<Renderer>().materials[0].SetTexture("_MainTex", tex);
        tex.Play();
    }
}
