using Assets.Modules.Core;
using Assets.Modules.Vision.CameraSources;
using UnityEngine;

namespace Assets.Modules.Vision
{
    [RequireComponent(typeof(OvrvisionCameraSource))]
    public class SaveOvrSettings : MonoBehaviour
    {
        public void Save()
        {
            var ovrCamera = GetComponent<OvrvisionCameraSource>();
            var settings = new OvrSettings
            {
                Gain = ovrCamera.Gain,
                Exposure = ovrCamera.Exposure,
                BLC = ovrCamera.BLC,
            };

            FileUtility.SaveToFile(Globals.OvrSettingsSavefile, JsonUtility.ToJson(settings));
        }
    }
}
