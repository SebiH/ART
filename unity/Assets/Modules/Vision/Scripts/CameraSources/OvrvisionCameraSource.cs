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
            // TODO: look up ovr::prop
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


        public Quality _camQuality = Quality.Q1280x960x45;
        public Quality CamQuality
        {
            get { return _camQuality; }
            set
            {
                if (_camQuality != value)
                {
                    _camQuality = value;

                    if (_isRunning)
                    {
                        Init();
                    }
                }
            }
        }


        public ProcessingMode _camMode = ProcessingMode.DemosaicRemap;
        public ProcessingMode CamMode
        {
            get { return _camMode; }
            set
            {
                if (_camMode != value)
                {
                    _camMode = value;

                    if (_isRunning)
                    {
                        Init();
                    }
                }
            }
        }

        public override void Init()
        {
            ImageProcessing.SetOvrCamera((int)_camQuality, (int)_camMode);
        }
    }
}
