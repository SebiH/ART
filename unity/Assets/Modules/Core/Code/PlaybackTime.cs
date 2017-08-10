using UnityEngine;

namespace Assets.Modules.Core
{
    public static class PlaybackTime
    {
        public static bool UseUnityTime = true;

        private static float _time = 0f;
        public static float RealTime
        {
            get
            {
                if (UseUnityTime) return Time.unscaledTime;
                else return _time;
            }
            set { _time = value; }
        }
    }
}
