using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Assets.Modules.Tracking
{
    /**
     *  Modified SteamVR_TrackedObject script.
     *  Changes:
     *  - Added option to turn off rotation/position tracking
     *  - Added delay parameter
     *
     *  Would be best to inherit from SteamVR_TrackedObject, but relevant methods are private
     */
    public class SteamVrDelayedTrackedObject : MonoBehaviour
    {
        public bool TrackRotation = true;
        public bool TrackPosition = true;

        [Range(0, 0.2f)]
        // Delay in ms
        public float TrackingDelay = 0f;

        private Queue<DelayedPose> _trackedPoses = new Queue<DelayedPose>();
        
        class DelayedPose
        {
            public float TimeOfPose;
            public SteamVR_Utils.RigidTransform Pose;

            public DelayedPose()
            {
                TimeOfPose = Time.time;
            }

            public DelayedPose(SteamVR_Utils.RigidTransform pose)
                : this()
            {
                Pose = pose;
            }
        };


        public enum EIndex
        {
            None = -1,
            Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
            Device1,
            Device2,
            Device3,
            Device4,
            Device5,
            Device6,
            Device7,
            Device8,
            Device9,
            Device10,
            Device11,
            Device12,
            Device13,
            Device14,
            Device15
        }

        public EIndex index;
        public Transform origin; // if not set, relative to parent
        public bool isValid = false;

        private void OnNewPoses(params object[] args)
        {
            if (index == EIndex.None)
                return;

            var i = (int)index;

            isValid = false;
            var poses = (Valve.VR.TrackedDevicePose_t[])args[0];
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            isValid = true;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                pose = new SteamVR_Utils.RigidTransform(origin) * pose;

                pose.pos.x *= origin.localScale.x;
                pose.pos.y *= origin.localScale.y;
                pose.pos.z *= origin.localScale.z;

                StashPose(pose);
            }
            else
            {
                StashPose(pose);
            }
        }

        private void StashPose(SteamVR_Utils.RigidTransform pose)
        {
            _trackedPoses.Enqueue(new DelayedPose(pose));
        }

        private void ApplyPose(SteamVR_Utils.RigidTransform pose)
        {
            if (TrackPosition)
            {
                transform.position = pose.pos;
            }

            if (TrackRotation)
            {
                transform.rotation = CalibrationOffset.OpenVrRotationOffset * pose.rot;
            }
        }


        void FixedUpdate()
        {
            // Fetch newest applicable pose
            var currentTime = Time.time;

            DelayedPose pose = null;

            while (_trackedPoses.Count > 0 && _trackedPoses.Peek().TimeOfPose + TrackingDelay < currentTime)
            {
                pose = _trackedPoses.Dequeue();
            }

            if (pose != null)
            {
                ApplyPose(pose.Pose);
            }
        }

        void OnEnable()
        {
            var render = SteamVR_Render.instance;
            if (render == null)
            {
                enabled = false;
                return;
            }

            SteamVR_Utils.Event.Listen("new_poses", OnNewPoses);
        }

        void OnDisable()
        {
            SteamVR_Utils.Event.Remove("new_poses", OnNewPoses);
            isValid = false;
        }

        public void SetDeviceIndex(int index)
        {
            if (System.Enum.IsDefined(typeof(EIndex), index))
                this.index = (EIndex)index;
        }
    }
}
