using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.CameraSources
{
    public class GoProCameraSource : CameraSource
    {
        public string Source = "10.5.5.9";
        public int Port = 8554;

        public override void InitCamera()
        {
            ImageProcessing.SetGoProCamera(Source, Port);
        }
    }
}
