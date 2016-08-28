using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override void InitCamera()
        {
            _prevCamQuality = CamQuality;
            _prevCamMode = CamMode;
            ImageProcessing.SetOvrCamera((int)CamQuality, (int)CamMode);
        }

        void Update()
        {
            bool hasModeChanged = (_prevCamMode != CamMode);
            bool hasQualityChanged = (_prevCamQuality != CamQuality);

            if (hasModeChanged || hasQualityChanged)
            {
                InitCamera();
            }
        }
    }
}
