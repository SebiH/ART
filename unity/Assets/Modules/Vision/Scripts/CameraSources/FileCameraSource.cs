using Assets.Modules.Core;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Vision.CameraSources
{
    public class FileCameraSource : CameraSource
    {
        private string _prevSourcePath = "";
        public string SourcePath = "";
        public bool Stop = false;

        public override void InitCamera()
        {
            PlaybackTime.UseUnityTime = false;

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
            if (!Stop)
            {
                PlaybackTime.RealTime = Time.unscaledTime;
            }

            if (_isRunning && SourcePath != _prevSourcePath)
            {
                _prevSourcePath = SourcePath;
                InitCamera();
            }
        }
    }
}
