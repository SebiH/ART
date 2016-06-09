using UnityEngine;
using Assets.Scripts.RealCamera;
using Assets.Code.Vision;

public class AttachCameraFeed : MonoBehaviour
{
    // TODO: only supports RawImage, needs some options for the others?
    public Module VisionModule = Module.RawImage;
    public OutputType Output = OutputType.Left;

    private int _textureHandle;

    void Start()
    {
        // TODO: might depend on module..
        var imageWidth = ImageProcessing.CameraWidth;
        var imageHeight = ImageProcessing.CameraHeight;

        // TODO: rendertexture?
        var camTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.BGRA32, false);
        camTexture.wrapMode = TextureWrapMode.Clamp;

        GetComponent<Renderer>().materials[0].SetTexture("_MainTex", camTexture);

        var texturePtr = camTexture.GetNativeTexturePtr();
        _textureHandle = ImageProcessing.AddTexturePtr(VisionModule, texturePtr, Output);
    }

    void OnDestroy()
    {
        ImageProcessing.RemoveTexturePtr(_textureHandle);
        _textureHandle = -1;
    }
}
