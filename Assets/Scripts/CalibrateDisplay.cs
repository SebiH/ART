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

    public BarObjectsGraph _calibratedObject;

    void Start()
    {
        _isCalibrated = false;
        _calibratedPoints.Clear();
    }
	
	void Update ()
    {
        if (!_isCalibrated)
        {
            var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
            var isTriggerPressed = (deviceIndex != -1) && SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.Trigger);
            var isKeyPressed = Input.GetKeyDown(KeyCode.Space);

            var isGripPressed = (deviceIndex != -1) && SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Grip);
            var isTouchpadPressed = (deviceIndex != -1) && SteamVR_Controller.Input(deviceIndex).GetPress(SteamVR_Controller.ButtonMask.Touchpad);

            if (isGripPressed)
            {
                if (isTriggerPressed)
                {
                    _calibratedObject.ClearBars();
                }
                else
                {
                    _calibratedObject.RegenerateGraph();
                }
            }

            if (isTriggerPressed)
            {
                //if (deviceIndex != -1)
                //{
                //    SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(1000);
                //}

                //SetPoint(transform.position);
                //CalibratePoints();

                // for initial testing
                //var testObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //testObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                //testObj.transform.position = transform.position;




                var axis0 = SteamVR_Controller.Input(deviceIndex).GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                var prevRotation = _calibratedObject.transform.rotation.eulerAngles;

                //if (Mathf.Abs(axis0.x) > 0.2 || Mathf.Abs(axis0.y) > 0.2)
                //{
                //    _calibratedObject.transform.rotation = Quaternion.Euler(prevRotation.x + axis0.x, prevRotation.y + axis0.y, prevRotation.z);
                //}
                //else 
                if (isTouchpadPressed)
                {
                    _calibratedObject.transform.position = transform.position - new Vector3(0, 0.08f, 0);
                }
            }
            else
            {

                if (isTouchpadPressed)
                {
                    var axis0 = SteamVR_Controller.Input(deviceIndex).GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                    var prevScale = _calibratedObject.transform.localScale;

                    //if (Mathf.Abs(axis0.x) > 0.2 || Mathf.Abs(axis0.y) > 0.2)
                    //{
                        _calibratedObject.transform.localScale = new Vector3(prevScale.x + axis0.x * 0.0005f, prevScale.y, prevScale.z + axis0.y * 0.0005f);
                    //}
                    //else
                    //{
                    //    _calibratedObject.HighlightRandomData();
                    //}
                }
            }
        }
    }


    private void SetPoint(Vector3 pos)
    {
        //var distThreshold = 0.1; // in meter
        //var nearbyPoints = _calibratedPoints.Where(@v => (@v - pos).sqrMagnitude <= distThreshold);

        //if (nearbyPoints.Any())
        //{
        //    // use mean of both points
        //    // TODO: don't use only first point
        //    pos = (pos + nearbyPoints.First()) / 2;
        //}

        //_calibratedPoints.Add(pos);

        if (_topLeftCorner == null)
        {
            _topLeftCorner = pos;
        }
        else if (_topRightCorner == null)
        {
            _topRightCorner = pos;
        }
        else if (_bottomLeftCorner == null)
        {
            _bottomLeftCorner = pos;
        }
        else if (_bottomRightCorner == null)
        {
            _bottomRightCorner = pos;
            _isCalibrated = true;
        }
    }


    private void CalibratePoints()
    {
        //if (_calibratedPoints.Count == 1)
        //{
        //    // TODO: nyi
        //}
        //else if (_calibratedPoints.Count > 1)
        //{
        //    // TODO: nyi

        //    if (_calibratedPoints.Count == 4)
        //    {
        //        _isCalibrated = true;
        //    }
        //}
       
    }
}
