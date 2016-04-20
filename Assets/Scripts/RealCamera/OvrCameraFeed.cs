using System;
using UnityEngine;

namespace Assets.Scripts.RealCamera
{
    class OvrCameraFeed : MonoBehaviour
    {
        // Camera GameObject
        private GameObject CameraPlaneLeft;
        private GameObject CameraPlaneRight;

        // Camera texture
        // TODO: should be rendertexture??
        private Texture2D CameraTexLeft = null;
        private Texture2D CameraTexRight = null;

        private int ImageWidth;
        private int ImageHeight;

        private int TextureHandleLeft;
        private int TextureHandleRight;

        private const float IMAGE_ZOFFSET = 0.02f;

        void Awake()
        {
            ImageProcessing.StartProcessing();
            ImageWidth = ImageProcessing.CameraWidth;
            ImageHeight = ImageProcessing.CameraHeight;
        }


        void Start()
        {
            // Initialize camera plane object(Left)
            var CameraLeft = transform.FindChild("LeftCamera").gameObject;
            var CameraRight = transform.FindChild("RightCamera").gameObject;
            CameraPlaneLeft = CameraLeft.transform.FindChild("LeftImagePlane").gameObject;
            CameraPlaneRight = CameraRight.transform.FindChild("RightImagePlane").gameObject;

            CameraLeft.transform.localPosition = Vector3.zero;
            CameraRight.transform.localPosition = Vector3.zero;
            CameraLeft.transform.localRotation = Quaternion.identity;
            CameraRight.transform.localRotation = Quaternion.identity;

            // Create cam texture
            CameraTexLeft = new Texture2D(ImageWidth, ImageHeight, TextureFormat.BGRA32, false);
            CameraTexRight = new Texture2D(ImageWidth, ImageHeight, TextureFormat.BGRA32, false);

            // Cam setting
            CameraTexLeft.wrapMode = TextureWrapMode.Clamp;
            CameraTexRight.wrapMode = TextureWrapMode.Clamp;

            // Mesh
            Mesh m = CreateCameraPlaneMesh();
            CameraPlaneLeft.GetComponent<MeshFilter>().mesh = m;
            CameraPlaneRight.GetComponent<MeshFilter>().mesh = m;

            CameraPlaneLeft.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexLeft);
            CameraPlaneRight.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexRight);

            var defaultFloatpoint = 0.427990019f;
            var defaultRightGap = new Vector3(0.0566581376f, -0.000236578562f, 0.001237078f);
            var defaultAspectW = 1.0105263f;

            var CameraRightGap = defaultRightGap;

            CameraPlaneLeft.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
            CameraPlaneRight.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
            CameraPlaneLeft.transform.localPosition = new Vector3(-0.032f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);
            CameraPlaneRight.transform.localPosition = new Vector3(CameraRightGap.x - 0.040f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);

            var LeftTexturePtr = CameraTexLeft.GetNativeTexturePtr();
            var RightTexturePtr = CameraTexRight.GetNativeTexturePtr();

            // TODO: store handle for deregister
            TextureHandleLeft = ImageProcessing.AddTexturePtr(ImageProcessing.MODULE_RAW_IMAGE, LeftTexturePtr, ImageProcessing.Type.left);
            TextureHandleRight = ImageProcessing.AddTexturePtr(ImageProcessing.MODULE_RAW_IMAGE, RightTexturePtr, ImageProcessing.Type.right);

            // quick hack to stop unity from crashing :(
            ImageProcessing.AddTextureSync(CameraTexLeft);
            ImageProcessing.AddTextureSync(CameraTexRight);
        }

        public void SetAlpha(float alpha)
        {
            CameraPlaneLeft.GetComponent<Renderer>().materials[0].color = new Color(1, 1, 1, alpha);
            CameraPlaneRight.GetComponent<Renderer>().materials[0].color = new Color(1, 1, 1, alpha);
        }

        void OnDestroy()
        {
            ImageProcessing.RemoveTexturePtr(TextureHandleLeft);
            TextureHandleLeft = -1;

            ImageProcessing.RemoveTexturePtr(TextureHandleRight);
            TextureHandleRight = -1;
        }

        private Mesh CreateCameraPlaneMesh()
        {
            Mesh m = new Mesh();
            m.name = "CameraImagePlane";
            Vector3[] vertices = new Vector3[]
            {
            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3( 0.5f,  0.5f, 0.0f),
            new Vector3( 0.5f, -0.5f, 0.0f),
            new Vector3(-0.5f,  0.5f, 0.0f)
            };
            int[] triangles = new int[]
            {
            0, 1, 2,
            1, 0, 3
            };
            Vector2[] uv = new Vector2[]
            {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f)
            };
            m.vertices = vertices;
            m.subMeshCount = 3;
            m.SetTriangles(triangles, 0);
            m.SetTriangles(triangles, 1);
            m.uv = uv;
            m.RecalculateNormals();

            return m;
        }

    }
}
