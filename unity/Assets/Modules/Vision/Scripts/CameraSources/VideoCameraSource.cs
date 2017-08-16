using Assets.Modules.Core;
using System;
using UnityEngine;

namespace Assets.Modules.Vision.CameraSources
{
    public class VideoCameraSource : CameraSource
    {
        public CurrentFilename file;
        public string SourceDir = "D:/videos/";
        public float TimeOffset = 0f;
        private static LockFreeQueue<double> _messages = new LockFreeQueue<double>();

        public bool IsPaused = false;
        private bool prevIsPaused = false;

        public override void InitCamera()
        {
            PlaybackTime.UseUnityTime = false;
            ImageProcessing.SetVideoCamera(SourceDir + file.Filename + ".mp4", SetTime);
        }

        private void FixedUpdate()
        {
            double time = 0;
            while (_messages.Dequeue(out time))
            {
                PlaybackTime.RealTime = (float)time + TimeOffset;
            }

            if (prevIsPaused != IsPaused)
            {
                prevIsPaused = IsPaused;
                ImageProcessing.SetCamJsonProperties(JsonUtility.ToJson(new CamProperties
                {
                    Pause = IsPaused
                }));
            }
        }

        private static void SetTime(double time)
        {
            _messages.Enqueue(time);
        }

        [Serializable]
        private struct CamProperties
        {
            public bool Pause;
        }
    }
}
