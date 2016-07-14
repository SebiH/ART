using UnityEngine;
using Assets.Scripts.RealCamera;
using Assets.Code.Vision;

public class AttachCameraFeed : MonoBehaviour
{
    // TODO: only supports RawImage, needs some options for the others?
    public Module VisionModule = Module.RawImage;
    public OutputType Output = OutputType.Left;

    // automatically aligns and scales object 
    public bool AutoAlign = true;

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

        if (AutoAlign)
        {
            var aspectRatio = new Vector2((float)(imageWidth) / (float)(imageHeight), -1);
            transform.localScale = new Vector3(aspectRatio.x, aspectRatio.y, 1.0f);

            float xOffset = 0;
            if (Output == OutputType.Right)
            {
                xOffset = -0.032f;
            }
            else if (Output == OutputType.Left)
            {
                xOffset = -0.032f; //ImageProcessing.GetHMDRightGap().x - 0.040f;
            }

            transform.localPosition = new Vector3(xOffset, 0.0f, ImageProcessing.CameraFocalPoint + 0.02f);
        }
    }

    void OnDestroy()
    {
        ImageProcessing.RemoveTexturePtr(_textureHandle);
        _textureHandle = -1;
    }
}
