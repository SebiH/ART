using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Vision.CameraSources
{
    public class FileCameraSource : CameraSource
    {
        private string _prevSourcePath = "";
        public string SourcePath = "";

        public override void InitCamera()
        {
            if (File.Exists(SourcePath))
            {
                ImageProcessing.SetFileCamera(SourcePath);
            }
            else
            {
                throw new Exception("Cannot open FileCamera, file " + SourcePath + " does not exist");
            }
        }

        protected void Update()
        {
            if (_isRunning && SourcePath != _prevSourcePath)
            {
                _prevSourcePath = SourcePath;
                InitCamera();
            }
        }
    }
}
