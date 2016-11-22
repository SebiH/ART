using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class FixedDisplay
    {
        private Vector3[] _calibratedCorners = new Vector3[4];

        public Resolution DisplayResolution { get; set; }

        // Returns position of center to match Unity
        public Vector3 Position
        {
            get { return MathUtility.Average(_calibratedCorners); }
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

                // invert rotation on diagonal so that forward == 0,0,1
                diagonal = Quaternion.Inverse(Rotation) * diagonal;

                var scale = new Vector3(Mathf.Abs(diagonal.x), 1f, Mathf.Abs(diagonal.z));
                return scale;
            }
        }

        public Vector3 Normal
        {
            get
            {
                var forward = Vector3.Normalize(_calibratedCorners[(int)Corner.TopLeft] - _calibratedCorners[(int)Corner.BottomLeft]);
                var right = Vector3.Normalize(_calibratedCorners[(int)Corner.BottomRight] - _calibratedCorners[(int)Corner.BottomLeft]);
                var up = Vector3.Cross(forward, right);
                return up;
            }
        }

        public FixedDisplay(Vector3 topleft, Vector3 bottomleft, Vector3 bottomright, Vector3 topright)
        {
            _calibratedCorners[(int)Corner.TopLeft] = topleft;
            _calibratedCorners[(int)Corner.BottomLeft] = bottomleft;
            _calibratedCorners[(int)Corner.BottomRight] = bottomright;
            _calibratedCorners[(int)Corner.TopRight] = topright;
        }


        public void SetCornerPosition(Corner c, Vector3 pos)
        {
            _calibratedCorners[(int)c] = pos;
        }

        public Vector3 GetCornerPosition(Corner c)
        {
            return _calibratedCorners[(int)c];
        }
    }
}
