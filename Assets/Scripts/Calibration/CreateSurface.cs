using UnityEngine;
using System.Collections;

public class CreateSurface : MonoBehaviour
{
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
        Debug.Log("Start surface");
    }

    private void UpdateSurfaceCreation(Vector3 position)
    {
        _endPoint = position;
        DrawLines();
        Debug.Log("Updating positions");
    }

    private void DrawLines()
    {
        var lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogWarning("Add LineRenderer for creating surfaces!");
            return;
        }

        var topLeft = _startPoint;
        var bottomRight = _endPoint;
        var dist = bottomRight - topLeft;
        Vector3 topRight, bottomLeft;

        if (Mathf.Abs(dist.y) < 0.2f)
        {
            topRight = new Vector3(_endPoint.x, _startPoint.y, _startPoint.z);
            bottomLeft = new Vector3(_startPoint.x, _startPoint.y, _endPoint.z);
            bottomRight.y = _startPoint.y;
        }
        else
        {
            topRight = new Vector3(_endPoint.x, _startPoint.y, _endPoint.z);
            bottomLeft = new Vector3(_startPoint.x, _endPoint.y, _startPoint.z);
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
