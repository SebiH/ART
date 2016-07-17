using System;
using UnityEngine;
using Valve.VR;

namespace Assets.Scripts
{
    /**
     *  Modified SteamVR_TrackedObject script.
     *  Changes:
     *  - Added option to turn off rotation/position tracking
     *  - Added delay parameter (TODO)
     *
     *  Would be best to inherit from SteamVR_TrackedObject, but relevant methods are private
     */
    public class DelayedSteamVrTracking : MonoBehaviour
    {
        public bool TrackRotation = true;
        public bool TrackPosition = true;

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

                ApplyPose(pose);
            }
            else
            {
                transform.localPosition = pose.pos;
                transform.localRotation = pose.rot;
            }
        }

        private void ApplyPose(SteamVR_Utils.RigidTransform pose)
        {
            if (TrackPosition)
            {
                transform.position = pose.pos;
            }

            if (TrackRotation)
            {
                transform.rotation = pose.rot;
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
