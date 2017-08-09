using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.CameraSources
{
    public class VideoCameraSource : CameraSource
    {
        public string Source = "";

        public override void InitCamera()
        {
            ImageProcessing.SetVideoCamera(Source);
        }
    }
}
