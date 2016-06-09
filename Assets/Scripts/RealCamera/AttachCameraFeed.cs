using UnityEngine;
using System.Collections;
using Assets.Scripts.RealCamera;
using Assets.Code.Vision;

public class AttachCameraFeed : MonoBehaviour
{
    private int texHandleLeft;
    private int texHandleRight;

	void Start ()
    {
        var ImageWidth = ImageProcessing.CameraWidth;
        var ImageHeight = ImageProcessing.CameraHeight;

        // TODO: rendertexture?
        var texLeft = new Texture2D(ImageWidth, ImageHeight, TextureFormat.BGRA32, false);
        texLeft.wrapMode = TextureWrapMode.Clamp;

        GetComponent<Renderer>().material.SetTexture("_MainTex", texLeft);

        var leftTexturePtr = texLeft.GetNativeTexturePtr();
        texHandleLeft = ImageProcessing.AddTexturePtr(Module.RawImage, leftTexturePtr, ImageProcessing.Type.left);
    }

    void OnDestroy()
    {
        ImageProcessing.RemoveTexturePtr(texHandleLeft);
        texHandleLeft = -1;
    }
	
	void Update ()
    {
	
	}
}
