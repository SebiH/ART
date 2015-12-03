using Assets.Code;
using UnityEditor;
using UnityEngine;


public class WebcamTexture : MonoBehaviour
{
    private Texture2D liveTexture;
    private int currentTextureGeneration;


    void Start()
    {
        CameraImageProvider.Init();
        var renderer = GetComponent<Renderer>();

        var width = 1280;
        var height = 720;

        liveTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        Color[] c = new Color[width*height];
        for (var i = 0; i < width * height; i++)
            c[i] = new Color(0, 1, 0);

        liveTexture.SetPixels(c);
        liveTexture.Apply();

        renderer.material.mainTexture = liveTexture;

        EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
    }

    private void OnPlaymodeChanged()
    {
        if (!EditorApplication.isPlaying)
        {
            CameraImageProvider.Stop();
        }
    }

    void Update()
    {
        var currentImageGeneration = CameraImageProvider.GetImageGeneration();

        if (currentImageGeneration > currentTextureGeneration)
        {
            liveTexture.LoadRawTextureData(CameraImageProvider.getCurrentImage());
            liveTexture.Apply();
            currentTextureGeneration = currentImageGeneration;
        }

    }

}
