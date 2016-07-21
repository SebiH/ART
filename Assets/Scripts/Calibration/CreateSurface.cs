using UnityEngine;
using System.Collections;

public class CreateSurface : MonoBehaviour
{
    // Object containing LineRenderer to simulate where the surface will be placed
    public GameObject Visualizer;

    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private bool _isCreatingSurface = false;

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
    }

    private void UpdateSurfaceCreation(Vector3 position)
    {
        _endPoint = position;
        PreviewSurface();
    }

    private void PreviewSurface()
    {
        if (Visualizer == null)
        {
            Debug.LogWarning("Add Visualiser object for previewing surfaces!");
            return;
        }

        var lineRenderer = Visualizer.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogWarning("Add LineRenderer to Visualiser for previewing surfaces!");
            return;
        }

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
        }

        lineRenderer.SetPositions(new[] { topLeft, topRight, bottomRight, bottomLeft, topLeft });
    }

    private void StopSurfaceCreation(Vector3 position)
    {
        if (_isCreatingSurface)
        {
            _isCreatingSurface = false;
        }
    }

}
