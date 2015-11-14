using Assets;
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

        liveTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);

        Color[] c = new Color[640*480];
        for (var i = 0; i < 640 * 480; i++)
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
        if (CameraImageProvider.GetImageGeneration() > currentTextureGeneration)
        {
            liveTexture.LoadRawTextureData(CameraImageProvider.getCurrentImage());
            liveTexture.Apply();
            currentTextureGeneration++;
        }

    }

}
