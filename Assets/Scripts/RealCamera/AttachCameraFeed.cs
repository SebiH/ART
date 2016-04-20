using UnityEngine;
using System.Collections;
using Assets.Scripts.RealCamera;

public class AttachCameraFeed : MonoBehaviour
{
    private int texHandleLeft;
    private int texHandleRight;

	void Start ()
    {
        ImageProcessing.StartProcessing();
        var ImageWidth = ImageProcessing.CameraWidth;
        var ImageHeight = ImageProcessing.CameraHeight;

        // TODO: rendertexture?
        var texLeft = new Texture2D(ImageWidth, ImageHeight, TextureFormat.BGRA32, false);
        texLeft.wrapMode = TextureWrapMode.Clamp;

        GetComponent<Renderer>().material.SetTexture("_MainTex", texLeft);

        var leftTexturePtr = texLeft.GetNativeTexturePtr();
        texHandleLeft = ImageProcessing.AddTexturePtr(ImageProcessing.MODULE_RAW_IMAGE, leftTexturePtr, ImageProcessing.Type.left);
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
