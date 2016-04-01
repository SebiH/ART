using System;
using UnityEngine;

namespace Assets.Scripts.RealCamera
{
    class OvrCameraWindow : MonoBehaviour
    {
        public int ImageWidth;
        public int ImageHeight;

        // max height depending on ovr camera feed - can't be more than is available!
        private int MaxImageWidth;
        private int MaxImageHeight;

        private const float IMAGE_ZOFFSET = 0.02f;

        void Awake()
        {
            ImageProcessing.OvrStart();
            MaxImageWidth = (int)ImageProcessing.GetProperty("width");
            MaxImageHeight = (int)ImageProcessing.GetProperty("height");
        }


        private GameObject CreatePlane()
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.transform.SetParent(transform);

            return plane;
        }

        void Start()
        {
            var CameraPlaneLeft = CreatePlane();
            var CameraPlaneRight = CreatePlane();

            // Create cam texture
            var imgWidth = Math.Min(ImageWidth, MaxImageWidth);
            var imgHeight = Math.Min(ImageHeight, MaxImageHeight);
            var CameraTexLeft = new Texture2D(imgWidth, imgHeight, TextureFormat.BGRA32, false);
            var CameraTexRight = new Texture2D(imgWidth, imgHeight, TextureFormat.BGRA32, false);

            // Cam setting
            CameraTexLeft.wrapMode = TextureWrapMode.Clamp;
            CameraTexRight.wrapMode = TextureWrapMode.Clamp;

            // Mesh
            Mesh m = CreateCameraPlaneMesh();
            CameraPlaneLeft.GetComponent<MeshFilter>().mesh = m;
            CameraPlaneRight.GetComponent<MeshFilter>().mesh = m;

            // SetShader
            CameraPlaneLeft.GetComponent<Renderer>().material.shader = Shader.Find("Ovrvision/ovTexture");
            CameraPlaneRight.GetComponent<Renderer>().material.shader = Shader.Find("Ovrvision/ovTexture");

            CameraPlaneLeft.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexLeft);
            CameraPlaneRight.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexRight);
            //CameraPlaneLeft.GetComponent<Renderer>().materials[1].SetTexture("_MainTex", CameraTexLeft);
            //CameraPlaneRight.GetComponent<Renderer>().materials[1].SetTexture("_MainTex", CameraTexRight);

            var defaultFloatpoint = 0.427990019f;
            var defaultRightGap = new Vector3(0.0566581376f, -0.000236578562f, 0.001237078f);
            var defaultAspectW = 1.0105263f;

            var CameraRightGap = defaultRightGap;

            CameraPlaneLeft.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
            CameraPlaneRight.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
            CameraPlaneLeft.transform.localPosition = new Vector3(-0.032f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);
            CameraPlaneRight.transform.localPosition = new Vector3(CameraRightGap.x - 0.040f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);

            leftTexturePtr = CameraTexLeft.GetNativeTexturePtr();
            rightTexturePtr = CameraTexRight.GetNativeTexturePtr();

        }

        private IntPtr leftTexturePtr, rightTexturePtr;

        void Update()
        {
            if (ImageProcessing.GetProperty("isOpen") >= 1.0f)
            {
                ImageProcessing.WriteTexture(new IntPtr(0), new IntPtr(0));
                ImageProcessing.WriteROITexture(0, 0, Math.Min(ImageWidth, MaxImageWidth), Math.Min(ImageHeight, MaxImageHeight), leftTexturePtr, rightTexturePtr);
            }
            else
            {
                Debug.LogError("Camera is not open!");
            }
        }

        void OnDestroy()
        {
            ImageProcessing.OvrStop();
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
