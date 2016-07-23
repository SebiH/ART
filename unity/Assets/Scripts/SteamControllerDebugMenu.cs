using System.Collections.Generic;
using UnityEngine;

public class SteamControllerDebugMenu : MonoBehaviour
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

            var isTouchpadActive = device.GetTouch(SteamVR_Controller.ButtonMask.Axis0);
            if (isTouchpadActive)
            {
                var axis0 = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
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
