using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class MarkerPose
    {
        public readonly int Id;
        public readonly string Name;
        public Matrix4x4 PoseMatrix;

        public Vector3 Position
        {
            get
            {
                var pos = PoseMatrix.GetPosition();
                // invert to match camera
                pos.y = -pos.y;
                return pos;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                var eulerRot = PoseMatrix.GetRotation().eulerAngles;
                eulerRot.x = -eulerRot.x;
                //eulerRot.y = -eulerRot.y;
                eulerRot.z = -eulerRot.z;

                return Quaternion.Euler(eulerRot);
            }
        }


        public Vector3 Scale
        {
            get
            {
                return PoseMatrix.GetScale();
            }
        }


        public MarkerPose(int id, string name, Matrix4x4 pose)
        {
            Id = id;
            Name = name;
            PoseMatrix = pose;
        }

        public MarkerPose Inverse()
        {
            return new MarkerPose(Id, Name, PoseMatrix.inverse);
        }
    }
}
