using Assets.Modules.Core;

namespace Assets.Modules.Vision.CameraSources
{
    public class VideoCameraSource : CameraSource
    {
        public string Source = "";
        public float TimeOffset = 0f;
        private static LockFreeQueue<double> _messages = new LockFreeQueue<double>();

        public override void InitCamera()
        {
            PlaybackTime.UseUnityTime = false;
            ImageProcessing.SetVideoCamera(Source, SetTime);
        }

        private void FixedUpdate()
        {
            double time = 0;
            while (_messages.Dequeue(out time))
            {
                PlaybackTime.RealTime = (float)time + TimeOffset;
            }
        }

        private static void SetTime(double time)
        {
            _messages.Enqueue(time);
        }
    }
}
