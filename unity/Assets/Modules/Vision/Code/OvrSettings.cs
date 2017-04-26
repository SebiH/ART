using System;

namespace Assets.Modules.Vision
{
    [Serializable]
    public struct OvrSettings
    {
        public int Gain;
        public int Exposure;
        public int BLC;

        public bool AutoContrast;
        public bool AutoContrastAutoGain;
        public float AutoContrastClipPercent;
        public float AutoContrastMax;

        public float CameraGap;
        public bool GapAutoAdjust;
    }
}
