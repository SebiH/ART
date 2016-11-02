using Assets.Modules.Core.Util;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class FixedDisplay
    {
        private Vector3[] _calibratedCorners = new Vector3[4];

        public Vector3 CenterPosition
        {
            get { return MathUtils.AverageVector(_calibratedCorners); }
        }

        public Quaternion Rotation
        {
            get
            {
                var forward = Vector3.Normalize(_calibratedCorners[(int)Corner.TopLeft] - _calibratedCorners[(int)Corner.BottomLeft]);
                var right = Vector3.Normalize(_calibratedCorners[(int)Corner.BottomRight] - _calibratedCorners[(int)Corner.BottomLeft]);
                var up = Vector3.Cross(forward, right);
                return Quaternion.LookRotation(forward, up);
            }
        }

        public Vector3 Scale
        {
            get
            {
                var diagonal = _calibratedCorners[(int)Corner.BottomRight] - _calibratedCorners[(int)Corner.TopLeft];

            }
        }


        public void SetCorner(Corner c, Vector3 pos)
        {

        }

        public Vector3 GetCorner(Corner c)
        {
            return _calibratedCorners[(int)c];
        }
    }
}
