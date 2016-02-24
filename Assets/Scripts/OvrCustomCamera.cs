using ComputerVision;
using UnityEngine;

public class OvrCustomCamera : MonoBehaviour
{
    public bool UseImageProcessing = false;

	// Camera GameObject
	private GameObject CameraLeft;
	private GameObject CameraRight;
	private GameObject CameraPlaneLeft;
	private GameObject CameraPlaneRight;
	// Camera texture
	private Texture2D CameraTexLeft = null;
	private Texture2D CameraTexRight = null;
	private Vector3 CameraRightGap;

    private int ImageWidth;
    private int ImageHeight;

    private int appliedImageId = -1;

	private const float IMAGE_ZOFFSET = 0.02f;

    void Awake()
    {
        ImageProcessor.Initialize();
        ImageWidth = ImageProcessor.Settings.imageSizeW;
        ImageHeight = ImageProcessor.Settings.imageSizeH;
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

        CameraRightGap = ImageProcessor.Settings.HMDCameraRightGap();

        // Plane reset
        CameraPlaneLeft.transform.localScale = new Vector3(ImageProcessor.Settings.aspectW, -1.0f, 1.0f);
        CameraPlaneRight.transform.localScale = new Vector3(ImageProcessor.Settings.aspectW, -1.0f, 1.0f);
        CameraPlaneLeft.transform.localPosition = new Vector3(-0.032f, 0.0f, ImageProcessor.Settings.GetFloatPoint() + IMAGE_ZOFFSET);
        CameraPlaneRight.transform.localPosition = new Vector3(CameraRightGap.x - 0.040f, 0.0f, ImageProcessor.Settings.GetFloatPoint() + IMAGE_ZOFFSET);
    }

    void Update()
    {
        ImageProcessor.FetchCurrentImage(CameraTexLeft.GetNativeTexturePtr(), CameraTexRight.GetNativeTexturePtr(), UseImageProcessing);
    }

    void OnDestroy()
    {
        ImageProcessor.Stop();
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
