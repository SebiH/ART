using Assets.Modules.Core;
using Assets.Modules.Vision.CameraSources;
using UnityEngine;

namespace Assets.Modules.Vision
{
    [RequireComponent(typeof(OvrvisionCameraSource), typeof(CameraGap))]
    public class SaveOvrSettings : MonoBehaviour
    {
        public void Save()
        {
            var ovrCamera = GetComponent<OvrvisionCameraSource>();
            var camGap = GetComponent<CameraGap>();
            var settings = new OvrSettings
            {
                Gain = ovrCamera.Gain,
                Exposure = ovrCamera.Exposure,
                BLC = ovrCamera.BLC,
                CameraGap = camGap.Gap,
                AutoContrast = ovrCamera.AutoContrast,
                AutoContrastClipPercent = ovrCamera.AutoContrastClipHistPercent,
                AutoContrastAutoGain = ovrCamera.AutoContrastAutoGain,
                AutoContrastMax = ovrCamera.AutoContrastMax,
                GapAutoAdjust = camGap.AutoAdjust
            };

            FileUtility.SaveToFile(Globals.OvrSettingsSavefile, JsonUtility.ToJson(settings));
        }
    }
}
