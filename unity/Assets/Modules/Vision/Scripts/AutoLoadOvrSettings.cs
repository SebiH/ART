using Assets.Modules.Core;
using Assets.Modules.Vision.CameraSources;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Vision
{
    [RequireComponent(typeof(OvrvisionCameraSource), typeof(CameraGap))]
    public class AutoLoadOvrSettings : MonoBehaviour
    {
        private void OnEnable()
        {
            StartCoroutine(LoadStartup());
        }

        private IEnumerator LoadStartup()
        {
            // wait until camera is initialized
            yield return new WaitForSeconds(5f);
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                var settingsJson = FileUtility.LoadFromFile(Globals.OvrSettingsSavefile);
                var settings = JsonUtility.FromJson<OvrSettings>(settingsJson);

                var ovrCamera = GetComponent<OvrvisionCameraSource>();
                ovrCamera.Gain = settings.Gain;
                ovrCamera.Exposure = settings.Exposure;
                ovrCamera.BLC = settings.BLC;

                var camGap = GetComponent<CameraGap>();
                camGap.Gap = settings.CameraGap;
                camGap.AutoAdjust = settings.GapAutoAdjust;

                Debug.Log("Successfully loaded ovr camera parameters");
            }
            catch (Exception e)
            {
                Debug.LogError("Could not autoload ovr camera parameters:");
                Debug.LogError(e.Message);
            }
        }
    }
}
