using Assets.Modules.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.CameraSources
{
    public class VideoCameraSource : CameraSource
    {
        public string Source = "";
        private double Time;

        public override void InitCamera()
        {
            PlaybackTime.UseUnityTime = false;
            ImageProcessing.SetVideoCamera(Source, SetTime);
        }

        private void SetTime(double time)
        {
            PlaybackTime.RealTime = (float)time;
        }
    }
}
