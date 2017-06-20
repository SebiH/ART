using Assets.Modules.Surfaces;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Modules.Tracking
{
    public class ViveCalibrateDisplay : MonoBehaviour
    {
        public string DisplayName = "Surface";
        public Vector3 Offset = Vector3.zero;

        // Set within editor
        public Corner CurrentCorner;

        public int MaxSamples = 100;
        //public float MaxPoseLag = 0.1f;

        public float BorderTop;
        public float BorderLeft;
        public float BorderRight;
        public float BorderBottom;
        public float HeightOffset;

        public float CalibrationProgress { get; private set; }
        public bool IsCalibrating { get; private set; }

        private Vector3[] _calibratedCorners = new Vector3[4];
        public readonly bool[] IsCornerCalibrated = new bool[4];

        void OnEnable()
        {
            IsCalibrating = false;
            CalibrationProgress = 0;
        }

        void OnDisable()
        {
        }

        private void Update()
        {
            transform.position = GetPosition();
        }


        private Vector3 GetPosition()
        {
            var pos = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.RightHand);
            var rot = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.RightHand);
            return pos + rot * Offset;
        }


        public void StartCalibration()
        {
            if (!IsCalibrating)
            {
                StartCoroutine(CalibrateCorner());
            }
        }

        private IEnumerator CalibrateCorner()
        {
            yield return new WaitForEndOfFrame();
            var samples = 0;
            var totalPositions = Vector3.zero;

            IsCalibrating = true;
            CalibrationProgress = 0f;

            while (samples < MaxSamples)
            {
                totalPositions += GetPosition();
                ++samples;
                CalibrationProgress = (float)samples / MaxSamples;
                yield return new WaitForSeconds(0.01f);
            }

            var avgPosition = totalPositions / MaxSamples;
            var cornerIndex = (int)CurrentCorner;
            _calibratedCorners[cornerIndex] = avgPosition;
            IsCornerCalibrated[cornerIndex] = true;

            IsCalibrating = false;
        }

        public void CommitFixedDisplay()
        {
            if (!IsCornerCalibrated.All((b) => b))
            {
                Debug.Log("Not all cornes calibrated yet");
                return;
            }

            SurfaceManager.Instance.Set(DisplayName,
                _calibratedCorners[(int)Corner.TopLeft] + new Vector3(BorderLeft, HeightOffset, -BorderTop),
                _calibratedCorners[(int)Corner.BottomLeft] + new Vector3(BorderLeft, HeightOffset, BorderBottom),
                _calibratedCorners[(int)Corner.BottomRight] + new Vector3(-BorderRight, HeightOffset, BorderBottom),
                _calibratedCorners[(int)Corner.TopRight] + new Vector3(-BorderRight, HeightOffset, -BorderTop));

            for (int i = 0; i < IsCornerCalibrated.Length; i++)
            {
                IsCornerCalibrated[i] = false;
            }
        }


        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {

                for (int i = 0; i < IsCornerCalibrated.Length; i++)
                {
                    if (IsCornerCalibrated[i])
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireSphere(_calibratedCorners[i], 0.01f);
                    }
                }
            }
        }
    }
}
