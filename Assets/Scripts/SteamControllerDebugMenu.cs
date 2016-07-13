using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CalibrateDisplay : MonoBehaviour
{
    public GameObject _selectionCollider;

    private List<GameObject> _possibleSelectedObjects = new List<GameObject>();
    private List<GameObject> _selectedObjects = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        _possibleSelectedObjects.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        _possibleSelectedObjects.Remove(other.gameObject);
    }



    void Start()
    {
    }
    
    void Update ()
    {
        var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);

        if (deviceIndex != -1)
        {
            var device = SteamVR_Controller.Input(deviceIndex);

            var isTriggerPressed = device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
            if (isTriggerPressed)
            {
                SetSelectedObjects(_possibleSelectedObjects);
            }

            var isTouchpadActive = device.GetTouchDown(EVRButtonId.k_EButton_Axis0);
            if (isTouchpadActive)
            {
                var axis1 = device.GetAxis(EVRAxisId.k_Axis1);
            }
        }


        if (!_isCalibrated)
        {
            var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
            var isTriggerPressed = (deviceIndex != -1) && SteamVR_Controller.Input(deviceIndex).GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
            var isKeyPressed = Input.GetKeyDown(KeyCode.Space);

            if (isTriggerPressed || isKeyPressed)
            {
                if (deviceIndex != -1)
                {
                    SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(1000);
                }

                SetPoint(transform.position);
                CalibratePoints();
                
                // for initial testing
                var testObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                testObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                testObj.transform.position = transform.position;
            }
        }
    }

    private void SetSelectedObjects(List<GameObject> selection)
    {
        foreach (var selObj in _selectedObjects)
        {
            // TODO remove highlight component
        }

        foreach (var selObj in selection)
        {
            // TODO set highlight
        }

        _selectedObjects.Clear();
        _selectedObjects.AddRange(selection);
    }

}
