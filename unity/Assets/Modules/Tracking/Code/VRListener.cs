using System;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public static class VRListener
    {
        public static Vector3 CurrentPosition { get; set; }
        public static Quaternion CurrentRotation { get; set; }
        public static float PoseUpdateTime { get; set; }



        public static event Action UpdateRequested;

        public static void TriggerUpdate()
        {
            if (UpdateRequested != null)
            {
                UpdateRequested();
            }
        }
    }
}
