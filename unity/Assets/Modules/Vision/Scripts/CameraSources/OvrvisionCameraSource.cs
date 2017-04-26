using System;
using System.Collections;
using UnityEngine;

namespace Assets.Modules.Vision.CameraSources
{
    public class OvrvisionCameraSource : CameraSource
    {
        public enum Quality
        {
            Q2560x1920x15 = 0,
            Q1920x1080x30 = 1,
            Q1280x960x45 = 2,
            Q960x950x60 = 3,
            Q1280x800x60 = 4,
            Q640x480x90 = 5,
            Q320x240x120 = 6,
        }

        public enum ProcessingMode
        {
            DemosaicRemap = 0,
            Demosaic = 1,
            None = 2
        }


        private Quality _prevCamQuality;
        public Quality CamQuality = Quality.Q1280x960x45;


        private ProcessingMode _prevCamMode;
        public ProcessingMode CamMode = ProcessingMode.DemosaicRemap; 


        [Serializable]
        private struct OvrSettings
        {
            public float[] HMDRightGap;
            public float FocalPoint;
            public int Exposure;
            public int Gain;
            public int BLC;
            public bool AutoWhiteBalance;
            public int WhiteBalanceR;
            public int WhiteBalanceG;
            public int WhiteBalanceB;
            public bool AutoContrast;
            public float AutoContrastClipHistPercent;
            public bool AutoContrastAutoGain;
            public float AutoContrastMax;
        }

        [Serializable]
        private struct OvrExposurePerSecondHelper
        {
            public float ExposurePerSec;
        }

        private OvrSettings _sourceSettings;

        [Range(1, 47)]
        public int Gain = 8;

        [Range(0, 32767)]
        public int Exposure = 12960;

        [Range(0, 1023)]
        public int BLC = 32;

        public bool AutoWhiteBalance = true;

        [Range(0, 4095)]
        public int WhiteBalanceR = 1474;

        [Range(0, 4095)]
        public int WhiteBalanceG = 1024;

        [Range(0, 4095)]
        public int WhiteBalanceB = 1738;

        public bool AutoContrast = true;
        public bool AutoContrastAutoGain = true;
        [Range(0.0f, 1.0f)]
        public float AutoContrastClipHistPercent = 0;
        [Range(1.0f, 20.0f)]
        public float AutoContrastMax = 0;

        public float ExposurePerSec
        {
            set
            {
                var helper = new OvrExposurePerSecondHelper { ExposurePerSec = value };
                ImageProcessing.SetCamJsonProperties(JsonUtility.ToJson(helper));

                ImageProcessing.GetCamJsonProperties(GetPropertyCallback);
                ReloadSettings();
            }
        }


        public float GetFocalPoint()
        {
            return _sourceSettings.FocalPoint * 0.001f; // mm -> m
        }

        public Vector3 GetHMDRightGap()
        {
            return new Vector3(_sourceSettings.HMDRightGap[0] * 0.001f, _sourceSettings.HMDRightGap[1] * 0.001f, _sourceSettings.HMDRightGap[2] * 0.001f); // mm -> m
        }

        public override void InitCamera()
        {
            _prevCamQuality = CamQuality;
            _prevCamMode = CamMode;
            ImageProcessing.SetOvrCamera((int)CamQuality, (int)CamMode);

            UpdateCameraProperties(true);
        }

#if UNITY_EDITOR
        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(FetchPropertiesPeriodically());
        }

        private IEnumerator FetchPropertiesPeriodically()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(0.3f);
                ReloadSettings();
            }
        }
#endif

        private void Update()
        {
            bool hasModeChanged = (_prevCamMode != CamMode);
            bool hasQualityChanged = (_prevCamQuality != CamQuality);

            if (hasModeChanged || hasQualityChanged)
            {
                InitCamera();
            }

            UpdateCameraProperties();
        }

        private void UpdateCameraProperties(bool force = false)
        {
            bool hasPropertyChanged = force;

            if (Exposure != _sourceSettings.Exposure)
            {
                _sourceSettings.Exposure = Exposure;
                hasPropertyChanged = true;
            }

            if (Gain != _sourceSettings.Gain)
            {
                _sourceSettings.Gain = Gain;
                hasPropertyChanged = true;
            }

            if (BLC != _sourceSettings.BLC)
            {
                _sourceSettings.BLC = BLC;
                hasPropertyChanged = true;
            }

            if (AutoWhiteBalance != _sourceSettings.AutoWhiteBalance)
            {
                _sourceSettings.AutoWhiteBalance = AutoWhiteBalance;
                hasPropertyChanged = true;
            }

            if (WhiteBalanceR != _sourceSettings.WhiteBalanceR)
            {
                _sourceSettings.WhiteBalanceR = WhiteBalanceR;
                hasPropertyChanged = true;
            }

            if (WhiteBalanceG != _sourceSettings.WhiteBalanceG)
            {
                _sourceSettings.WhiteBalanceG = WhiteBalanceG;
                hasPropertyChanged = true;
            }

            if (WhiteBalanceB != _sourceSettings.WhiteBalanceB)
            {
                _sourceSettings.WhiteBalanceB = WhiteBalanceB;
                hasPropertyChanged = true;
            }

            if (AutoContrast != _sourceSettings.AutoContrast)
            {
                _sourceSettings.AutoContrast = AutoContrast;
                hasPropertyChanged = true;
            }

            if (AutoContrastAutoGain != _sourceSettings.AutoContrastAutoGain)
            {
                _sourceSettings.AutoContrastAutoGain = AutoContrastAutoGain;
                hasPropertyChanged = true;
            }

            if (AutoContrastClipHistPercent != _sourceSettings.AutoContrastClipHistPercent)
            {
                _sourceSettings.AutoContrastClipHistPercent = AutoContrastClipHistPercent;
                hasPropertyChanged = true;
            }

            if (AutoContrastMax != _sourceSettings.AutoContrastMax)
            {
                _sourceSettings.AutoContrastMax = AutoContrastMax;
                hasPropertyChanged = true;
            }

            if (hasPropertyChanged)
            {
                // apply settings
                var newConfig = JsonUtility.ToJson(_sourceSettings);
                ImageProcessing.SetCamJsonProperties(newConfig);
                ReloadSettings();
            }
        }

        private void ReloadSettings()
        {
            // load new settings
            ImageProcessing.GetCamJsonProperties(GetPropertyCallback);

            // make sure settings are synced, in case a value was out of range
            Gain = _sourceSettings.Gain;
            Exposure = _sourceSettings.Exposure;
            BLC = _sourceSettings.BLC;
            AutoWhiteBalance = _sourceSettings.AutoWhiteBalance;
            WhiteBalanceR = _sourceSettings.WhiteBalanceR;
            WhiteBalanceG = _sourceSettings.WhiteBalanceG;
            WhiteBalanceB = _sourceSettings.WhiteBalanceB;
            AutoContrast = _sourceSettings.AutoContrast;
            AutoContrastAutoGain = _sourceSettings.AutoContrastAutoGain;
            AutoContrastClipHistPercent = _sourceSettings.AutoContrastClipHistPercent;
            AutoContrastMax = _sourceSettings.AutoContrastMax;
        }

        private void GetPropertyCallback(string json_properties_str)
        {
            _sourceSettings = JsonUtility.FromJson<OvrSettings>(json_properties_str);
        }

    }
}
