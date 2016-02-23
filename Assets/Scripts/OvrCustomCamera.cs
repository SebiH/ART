using UnityEngine;
using System.Collections;
using Assets.Code;

public class OvrCustomCamera : MonoBehaviour
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

    // TODO: get camera image size from OVRVision?
    private int ImageWidth = 960;
    private int ImageHeight = 950;

    private int appliedImageId = -1;

	private const float IMAGE_ZOFFSET = 0.02f;

    void Awake()
    {
        CameraImageProvider.Start();
    }


    void Start()
    {
        if (!CameraImageProvider.IsRunning)
        {
            Debug.Log("Unable to start: CameraImageProvider is not running!");
            return;
        }

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

        // OvrPro is initialized elsewhere, so we'll just use standard parameters for 860x850
        // TODO: maybe provide this via api? current problem is that the initialisation is async

        // OvrPro.GetFloatPoint()
        var defaultFloatpoint = 0.427990019f;
        // OvrPro.HMDCameraRightGap();
        var defaultRightGap = new Vector3(0.0566581376f, -0.000236578562f, 0.001237078f);
        // OvrPro.aspectW
        var defaultAspectW = 1.0105263f;

        CameraRightGap = defaultRightGap;

        // Plane reset
        CameraPlaneLeft.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
        CameraPlaneRight.transform.localScale = new Vector3(defaultAspectW, -1.0f, 1.0f);
        CameraPlaneLeft.transform.localPosition = new Vector3(-0.032f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);
        CameraPlaneRight.transform.localPosition = new Vector3(CameraRightGap.x - 0.040f, 0.0f, defaultFloatpoint + IMAGE_ZOFFSET);
    }

    void Update()
    {
        if (CameraImageProvider.IsRunning)
        {
            // avoid unnecessary updates by only updating if there actually is an updated image available
            var currentImageId = CameraImageProvider.GetCurrentImageId();

            if (currentImageId > appliedImageId)
            {
                CameraTexLeft.LoadRawTextureData(CameraImageProvider.GetLeftRawImage());
                CameraTexLeft.Apply();

                CameraTexRight.LoadRawTextureData(CameraImageProvider.GetRightRawImage());
                CameraTexRight.Apply();

                appliedImageId = currentImageId;
            }
        }
        else
        {
            Debug.LogError("Camera Provider isn't running");
        }
    }

    void OnDestroy()
    {
        CameraImageProvider.Stop();
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
