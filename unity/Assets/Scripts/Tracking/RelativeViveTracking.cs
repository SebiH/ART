using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Assets.Scripts
{
    /**
     *  Modified SteamVR_TrackedObject script.
     *  Changes:
     *      - Tracking is relative to specified object, e.g. to track controllers in relation to headset, so that
     *        we may use the controllers even though headtracking is done via a different system
     *
     *  Would be best to inherit from SteamVR_TrackedObject, but relevant methods are private
     */
    public class RelativeViveTracking : MonoBehaviour
   {
        public EIndex relativeToIndex;
        public Vector3 RotationOffset = Vector3.zero;
        public Transform AttachTo;

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
            if (index == EIndex.None || relativeToIndex == EIndex.None)
                return;

            var i = (int)index;
            var relIndex = (int)relativeToIndex;

            isValid = false;
            var poses = (Valve.VR.TrackedDevicePose_t[])args[0];
            if (poses.Length <= Math.Max(i, relIndex))
                return;

            if (!poses[i].bDeviceIsConnected || !poses[relIndex].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid || !poses[relIndex].bPoseIsValid)
                return;

            isValid = true;

            var relPose = new SteamVR_Utils.RigidTransform(poses[relIndex].mDeviceToAbsoluteTracking);
            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                pose = new SteamVR_Utils.RigidTransform(origin) * pose;

                pose.pos.x *= origin.localScale.x;
                pose.pos.y *= origin.localScale.y;
                pose.pos.z *= origin.localScale.z;

                ApplyPose(relPose, pose);
            }
            else
            {
                ApplyPose(relPose, pose);
            }
        }

        private void ApplyPose(SteamVR_Utils.RigidTransform relPose, SteamVR_Utils.RigidTransform pose)
        {
            var newPos = pose.pos - relPose.pos;
            //newPos.z = -newPos.z;
            //newPos.x = -newPos.x;
            transform.position = AttachTo.position + newPos;
            transform.rotation = Quaternion.Euler(pose.rot.eulerAngles + RotationOffset);
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
            {
                this.index = (EIndex)index;
            }
        }
    } 
}
