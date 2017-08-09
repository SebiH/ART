using Assets.Modules.Core;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.VR;

namespace Assets.Modules.Tracking
{
    public class RecordUnityVr : MonoBehaviour
    {
        public string Filename;
        private StreamWriter _cache;

        private void OnEnable()
        {
            _cache = File.AppendText(FileUtility.GetPath(Filename));
            _cache.AutoFlush = true;
            _cache.WriteLine("{ \"entries\": [");
        }

        private void OnDisable()
        {
            _cache.WriteLine("]}");
            _cache.Close();
        }


        private void FixedUpdate()
        {
            var pose = new RecordedPose
            {
                Time = Time.unscaledTime,
                HeadRot = InputTracking.GetLocalRotation(VRNode.Head),
                HeadPos = InputTracking.GetLocalPosition(VRNode.Head),
                LHRot = InputTracking.GetLocalRotation(VRNode.LeftHand),
                LHPos = InputTracking.GetLocalPosition(VRNode.LeftHand),
                RHRot = InputTracking.GetLocalRotation(VRNode.RightHand),
                RHPos = InputTracking.GetLocalPosition(VRNode.RightHand),
            };
            _cache.WriteLine(JsonUtility.ToJson(pose) + ";");
        }

        [Serializable]
        public struct RecordedPose
        {
            public float Time;
            public Quaternion HeadRot;
            public Vector3 HeadPos;
            public Quaternion LHRot;
            public Vector3 LHPos;
            public Quaternion RHRot;
            public Vector3 RHPos;
        }
    }
}
