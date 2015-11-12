using Assets;
using UnityEngine;

using System.Reflection;
using System;
/**
*  Taken from https://github.com/marcteys/oculus-webcams-unity/
*/
public class WebcamTexture : MonoBehaviour
{
    private Texture2D liveTexture;
    private int currentTextureGeneration;

    void Start()
    {
        CameraImageProvider.Init();
        var renderer = GetComponent<Renderer>();

        liveTexture = new Texture2D(640, 480);

        Color[] c = new Color[640*480];
        for (var i = 0; i < 640 * 480; i++)
            c[i] = new Color(0, 1, 0);

        liveTexture.SetPixels(c);
        liveTexture.Apply();

        renderer.material.mainTexture = liveTexture;
    }

    //void FitScreen()
    //{
    //    Camera cam = transform.parent.GetComponent<Camera>();

    //    float height = cam.orthographicSize * 2.0f;
    //    float width = height * Screen.width / Screen.height;
    //    float fix = 0;

    //    if (width > height) fix = width + margin;
    //    if (width < height) fix = height + margin;
    //    transform.localScale = new Vector3((fix / scaleFactor) * 4 / 3, fix / scaleFactor, 0.1f);
    //}

    void Update()
    {
        if (CameraImageProvider.GetImageGeneration() > currentTextureGeneration)
        {
            liveTexture.LoadRawTextureData(CameraImageProvider.getCurrentImage());
            currentTextureGeneration++;
        }

    }

}
