using System;

namespace Assets.Modules.Vision
{
    [Serializable]
    public struct OvrSettings
    {
        public int Gain;
        public int Exposure;
        public int BLC;

        public float CameraGap;
        public bool GapAutoAdjust;
    }
}
