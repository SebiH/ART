using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Modules.Vision.CameraSources
{
    public class OpenCVCameraSource : CameraSource
    {
        public override void InitCamera()
        {
            ImageProcessing.SetOpenCVCamera();
        }
    }
}
