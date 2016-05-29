using UnityEngine;
using System.Collections;
using Assets.Scripts.RealCamera;

public class UvTest : MonoBehaviour
{
    [Range(0, 1)]
    public float x1 = 0f;
    [Range(0, 1)]
    public float z1 = 0f;
    [Range(0, 1)]
    public float x2 = 1f;
    [Range(0, 1)]
    public float z2 = 1f;
    [Range(0, 1)]
    public float x3 = 1f;
    [Range(0, 1)]
    public float z3 = 0f;
    [Range(0, 1)]
    public float x4 = 0f;
    [Range(0, 1)]
    public float z4 = 1f;

    private int texHandleLeft;
    private int texHandleRight;

    void Start()
    {
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

    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        if (uvs.Length == 4)
        {
            uvs[0] = new Vector2(x1, z1);
            uvs[1] = new Vector2(x2, z2);
            uvs[2] = new Vector2(x3, z3);
            uvs[3] = new Vector2(x4, z4);
        }

        //for (int i = 0; i < uvs.Length; i++)
        //{
        //    uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        //}

        mesh.uv = uvs;
    }
}
