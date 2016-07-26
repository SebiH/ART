using UnityEngine;
using System.Collections;

public class CreateSurface : MonoBehaviour
{
    // Object containing LineRenderer to simulate where the surface will be placed
    public GameObject Visualizer;
    public GameObject SurfacePrefab;

    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private bool _isCreatingSurface = false;
    private bool _isSurfaceHorizontal;

    void Update()
    {
        var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
        var isTriggerDown = (deviceIndex != -1) && SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
        var isMouseDown = Input.GetKeyDown(KeyCode.Space); //Input.GetMouseButtonDown(0);

        var isTriggerPressed = (deviceIndex != -1) && SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger);
        var isMousePressed = Input.GetKey(KeyCode.Space); //Input.GetMouseButton(0);


        if (isTriggerDown || isMouseDown)
        {
            if (deviceIndex != -1)
            {
                SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(1000);
            }

            StartSurfaceCreation(transform.position);
        }
        else if (isTriggerPressed || isMousePressed)
        {
            UpdateSurfaceCreation(transform.position);
        }
        else
        {
            StopSurfaceCreation(transform.position);
        }
    }

    private void StartSurfaceCreation(Vector3 position)
    {
        _startPoint = position;
        _isCreatingSurface = true;
        
        Visualizer.GetComponent<ParticleSystem>().Play();
        var lineRenderer = Visualizer.GetComponent<LineRenderer>();
        lineRenderer.enabled = true;
        lineRenderer.SetPositions(new[] { transform.position, transform.position, transform.position, transform.position, transform.position, });
    }

    private void UpdateSurfaceCreation(Vector3 position)
    {
        _endPoint = position;
        PreviewSurface();
    }

    private void PreviewSurface()
    {
        var lineRenderer = Visualizer.GetComponent<LineRenderer>();

        var topLeft = _startPoint;
        var bottomRight = _endPoint;
        var dist = bottomRight - topLeft;
        Vector3 topRight, bottomLeft;


        if (Mathf.Abs(dist.y) < 0.2f)
        {
            // create horizontal (e.g. table) surface
            topRight = new Vector3(_endPoint.x, _startPoint.y, _startPoint.z);
            bottomLeft = new Vector3(_startPoint.x, _startPoint.y, _endPoint.z);
            bottomRight.y = _startPoint.y;

            Visualizer.transform.position = _startPoint + new Vector3(dist.x / 2,  0, dist.z / 2);
            Visualizer.transform.rotation = Quaternion.identity;
            Visualizer.transform.localScale = new Vector3(dist.x, 0.03f, dist.z);
            _isSurfaceHorizontal = true;
        }
        else
        {
            // create vertical (e.g. display) surface
            topRight = new Vector3(_endPoint.x, _startPoint.y, _endPoint.z);
            bottomLeft = new Vector3(_startPoint.x, _endPoint.y, _startPoint.z);

            var rotation = Vector2.Angle(new Vector2(0, 1), new Vector2(dist.x, dist.z));
            if (dist.x < 0)
                rotation = -rotation;
            Visualizer.transform.rotation = Quaternion.Euler(0, rotation, 0);
            Visualizer.transform.position = _startPoint + new Vector3(dist.x / 2,  dist.y / 2, dist.z / 2);
            Visualizer.transform.localScale = new Vector3(0.03f, dist.y,  new Vector2(dist.z, dist.x).magnitude);
            _isSurfaceHorizontal = false;
        }

        lineRenderer.SetPositions(new[] { topLeft, topRight, bottomRight, bottomLeft, topLeft });
    }

    private GameObject _prevCreatedSurface = null;

    private void StopSurfaceCreation(Vector3 position)
    {
        if (_isCreatingSurface)
        {
            _isCreatingSurface = false;

            Visualizer.GetComponent<ParticleSystem>().Stop();
            //Visualizer.GetComponent<LineRenderer>().enabled = false;
            var createdSurface = Instantiate(SurfacePrefab, Visualizer.transform.position, Visualizer.transform.rotation) as GameObject;
            createdSurface.transform.localScale = Visualizer.transform.localScale;

            // haaaaack
            var surfaceClient = createdSurface.GetComponent<InteractiveSurfaceClient>();
            if (surfaceClient != null && !_isSurfaceHorizontal)
            {
                surfaceClient.Cursor.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                surfaceClient.IsVertical = true;
            }

            if (_prevCreatedSurface != null)
            {
                Destroy(_prevCreatedSurface);
            }
            _prevCreatedSurface = createdSurface;
        }
    }

}
