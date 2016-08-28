using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.Vision.CameraSources
{
    public class DummyCameraSource : CameraSource
    {
        private string _prevSourcePath = "";
        public string SourcePath = "";

        //private string _sourcePath = "";
        //public string SourcePath
        //{
        //    get { return _sourcePath; }
        //    set
        //    {
        //        if (_sourcePath != value)
        //        {
        //            _sourcePath = value;

        //            if (_isRunning)
        //            {
        //                Init();
        //            }
        //        }
        //    }
        //}

        public override void Init()
        {
            if (File.Exists(SourcePath))
            {
                ImageProcessing.SetDummyCamera(SourcePath);
            }
            else
            {
                throw new Exception("Cannot open DummyCamera, file " + SourcePath + " does not exist");
            }
        }

        void Update()
        {
            if (_isRunning && SourcePath != _prevSourcePath)
            {
                Init();
            }
        }
    }
}
