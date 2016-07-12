using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CalibrateDisplay : MonoBehaviour
{
    private readonly List<Vector3> _calibratedPoints = new List<Vector3>();
    private bool _isCalibrated;

    private Vector3 _topLeftCorner;
    private Vector3 _topRightCorner;
    private Vector3 _bottomLeftCorner;
    private Vector3 _bottomRightCorner;

    void Start()
    {
        _isCalibrated = false;
        _calibratedPoints.Clear();
    }
	
	void Update ()
    {
        if (!_isCalibrated)
        {
            var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
            var isTriggerPressed = deviceIndex != -1 && SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
            var isKeyPressed = Input.GetKeyDown(KeyCode.Space);

            if (isTriggerPressed || isKeyPressed)
            {
                SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(1000);
                SetPoint(transform.position);
                CalibratePoints();
                
                // for initial testing
                var testObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                testObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                testObj.transform.position = transform.position;
            }
        }
    }


    private void SetPoint(Vector3 pos)
    {
        var distThreshold = 0.1; // in meter
        var nearbyPoints = _calibratedPoints.Where(@v => (@v - pos).sqrMagnitude <= distThreshold);

        if (nearbyPoints.Any())
        {
            // use mean of both points
            // TODO: don't use only first point
            pos = (pos + nearbyPoints.First()) / 2;
        }

        _calibratedPoints.Add(pos);
    }


    private void CalibratePoints()
    {
        if (_calibratedPoints.Count == 1)
        {
            // TODO: nyi
        }
        else if (_calibratedPoints.Count > 1)
        {
            // TODO: nyi

            if (_calibratedPoints.Count == 4)
            {
                _isCalibrated = true;
            }
        }
       
    }
}
