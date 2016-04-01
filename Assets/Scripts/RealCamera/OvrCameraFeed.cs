using System;
using UnityEngine;

namespace Assets.Scripts.RealCamera
{
    class OvrCameraFeed : MonoBehaviour
    {
        // Camera GameObject
        private GameObject CameraLeft;
        private GameObject CameraRight;
        private GameObject CameraPlaneLeft;
        private GameObject CameraPlaneRight;
        // Camera texture
        private Texture2D CameraTexLeft = null;
        private Texture2D CameraTexRight = null;
        private Vector3 CameraRightGap;
        private IntPtr LeftTexturePtr;
        private IntPtr RightTexturePtr;

        private int ImageWidth;
        private int ImageHeight;

        private const float IMAGE_ZOFFSET = 0.02f;

        void Awake()
        {
            ImageProcessing.Instance.RequestStart();
            ImageWidth = (int)ImageProcessing.Instance.GetCameraProperty("width");
            ImageHeight = (int)ImageProcessing.Instance.GetCameraProperty("height");
        }


        void Start()
        {
            // Initialize camera plane object(Left)
            CameraLeft = transform.FindChild("LeftCamera").gameObject;
            CameraRight = transform.FindChild("RightCamera").gameObject;
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

            // SetShader
            CameraLeft.GetComponent<Camera>().enabled = true;
            CameraRight.GetComponent<Camera>().enabled = true;

            CameraPlaneLeft.GetComponent<Renderer>().material.shader = Shader.Find("Ovrvision/ovTexture");
            CameraPlaneRight.GetComponent<Renderer>().material.shader = Shader.Find("Ovrvision/ovTexture");

            CameraPlaneLeft.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexLeft);
            CameraPlaneRight.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexRight);
            CameraPlaneLeft.GetComponent<Renderer>().materials[1].SetTexture("_MainTex", CameraTexLeft);
            CameraPlaneRight.GetComponent<Renderer>().materials[1].SetTexture("_MainTex", CameraTexRight);

            var defaultFloatpoint = 0.427990019f;
            var defaultRightGap = new Vector3(0.0566581376f, -0.000236578562f, 0.001237078f);
            var defaultAspectW = 1.0105263f;

            CameraRightGap = defaultRightGap;

            CameraPlaneLeft.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
            CameraPlaneRight.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
            CameraPlaneLeft.transform.localPosition = new Vector3(-0.032f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);
            CameraPlaneRight.transform.localPosition = new Vector3(CameraRightGap.x - 0.040f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);

            LeftTexturePtr = CameraTexLeft.GetNativeTexturePtr();
            RightTexturePtr = CameraTexRight.GetNativeTexturePtr();

            ImageProcessing.Instance.RegisterTextureUpdate(ImageProcessing.ImageProcessingMethod.Native, LeftTexturePtr, RightTexturePtr);
        }

        void OnDestroy()
        {
            ImageProcessing.Instance.DeregisterTexture(LeftTexturePtr, RightTexturePtr);
            ImageProcessing.Instance.RequestShutdown();
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
            m.subMeshCount = 2;
            m.SetTriangles(triangles, 0);
            m.SetTriangles(triangles, 1);
            m.uv = uv;
            m.RecalculateNormals();

            return m;
        }

    }
}
