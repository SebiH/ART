using Assets.Modules.Core;
using UnityEngine;

namespace Assets.Modules.Surfaces
{
    public class Surface : MonoBehaviour
    {
        public float PixelToCmRatio = 0.0485f; // measured 2016-11-15 on Microsoft Surface
        public Resolution DisplayResolution { get; set; }
        public string ClientName = "";

        public delegate void SurfaceActionHandler(string cmd, string payload);
        public event SurfaceActionHandler OnAction;

        private Vector3[] _calibratedCorners = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        #region Calculated Properties
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

        #endregion

        public Surface()
        {
        }

        public void SetCornerPosition(Corner c, Vector3 pos)
        {
            _calibratedCorners[(int)c] = pos;
            UpdateTransform();
        }

        public Vector3 GetCornerPosition(Corner c)
        {
            return _calibratedCorners[(int)c];
        }


        public float PixelToUnityCoord(float pixelLocation)
        {
            // pixel coords uses cm, unity uses m
            return PixelToCmRatio * pixelLocation / 100f;           
        }

        public float UnityToPixelLocation(float unityCoord)
        {
            // pixel coords uses cm, unity uses m
            return (1 / PixelToCmRatio) * unityCoord * 100f;
        }


        public void TriggerAction(string cmd, string payload)
        {
            if (OnAction != null)
            {
                OnAction(cmd, payload);
            }
        }

        public void UpdateTransform()
        {
            transform.position = GetCornerPosition(Corner.BottomLeft);
            transform.rotation = Rotation;
            transform.localScale = Vector3.one;
        }

        void OnDrawGizmos()
        {
            // draw coordinates as given via calibration
            Gizmos.color = Color.white;

            var tl = GetCornerPosition(Corner.TopLeft);
            var tr = GetCornerPosition(Corner.TopRight);
            var bl = GetCornerPosition(Corner.BottomLeft);
            var br = GetCornerPosition(Corner.BottomRight);

            Gizmos.DrawLine(tl, bl);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);

            // draw coordinates as calculated via pixelToCm Ratio and displaySize
            Gizmos.color = Color.red;
            var uh = PixelToUnityCoord(DisplayResolution.height);
            var uw = PixelToUnityCoord(DisplayResolution.width);

            var downV = (bl - tl).normalized;
            var rightV = (tr - tl).normalized;

            Gizmos.DrawLine(tl, tl + downV * uh);
            Gizmos.DrawLine(tl + downV * uh, tl + downV * uh + rightV * uw);
            Gizmos.DrawLine(tl + downV * uh + rightV * uw, tl + rightV * uw);
            Gizmos.DrawLine(tl, tl + rightV * uw);
        }
    }
}
