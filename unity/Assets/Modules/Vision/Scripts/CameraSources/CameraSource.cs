using System;
using UnityEngine;

namespace Assets.Modules.Vision.CameraSources
{
    public abstract class CameraSource : MonoBehaviour
    {
        protected bool _isRunning = false;

        public virtual void Start()
        {
            if (!_isRunning)
            {
                try
                {
                    _isRunning = true;
                    InitCamera();

                    LoadCurrentProperties();
                    UpdateCameraProperties(true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public abstract void InitCamera();

        public virtual void Stop()
        {
            _isRunning = false;
        }

        protected virtual void Update()
        {
            if (_isRunning)
            {
                UpdateCameraProperties();
            }
        }


        #region Camera Properties

        [Range(1, 47)]
        public int Gain = 8;
        private int _sourceGain;

        [Range(0, 32767)]
        public int Exposure = 12960;
        private int _prevExposure;

        [Range(0, 1023)]
        public int BLC = 32;
        private int _prevBLC;

        public bool AutoWhiteBalance = true;
        private bool _prevAutoWhiteBalance;

        [Range(0, 4095)]
        public int WhiteBalanceR = 1474;
        private int _prevWhiteBalanceR;

        [Range(0, 4095)]
        public int WhiteBalanceG = 1024;
        private int _prevWhiteBalanceG;

        [Range(0, 4095)]
        public int WhiteBalanceB = 1738;
        private int _prevWhiteBalanceB;

        private void LoadCurrentProperties()
        {
            //_prevGain = SourceGain;
            //_prevExposure = SourceExposure;
            //_prevBLC = SourceBLC;

            //_prevAutoWhiteBalance = SourceAutoWhiteBalance;
            //_prevWhiteBalanceR = SourceWhiteBalanceR;
            //_prevWhiteBalanceG = SourceWhiteBalanceG;
            //_prevWhiteBalanceB = SourceWhiteBalanceB;
        }

        public void UpdateCameraProperties(bool forceUpdate = false)
        {
            //if (_prevGain != Gain || forceUpdate)
            //{
            //    SourceGain = Gain;
            //    _prevGain = Gain;
            //}

            //if (_prevExposure != Exposure || forceUpdate)
            //{
            //    SourceExposure = _prevExposure;
            //    _prevExposure = Exposure;
            //}

            //if (_prevBLC != BLC || forceUpdate)
            //{
            //    SourceBLC = BLC;
            //    _prevBLC = BLC;
            //}

            //if (_prevAutoWhiteBalance != AutoWhiteBalance || forceUpdate)
            //{
            //    SourceAutoWhiteBalance = AutoWhiteBalance;
            //    _prevAutoWhiteBalance = AutoWhiteBalance;
            //}

            //if (_prevWhiteBalanceR != WhiteBalanceR || forceUpdate)
            //{
            //    SourceWhiteBalanceR = WhiteBalanceR;
            //    _prevWhiteBalanceR = WhiteBalanceR;
            //}

            //if (_prevWhiteBalanceG != WhiteBalanceG || forceUpdate)
            //{
            //    SourceWhiteBalanceG = WhiteBalanceG;
            //    _prevWhiteBalanceG = WhiteBalanceG;
            //}

            //if (_prevWhiteBalanceB != WhiteBalanceB || forceUpdate)
            //{
            //    SourceWhiteBalanceB = WhiteBalanceB;
            //    _prevWhiteBalanceB = WhiteBalanceB;
            //}
        }


        public Vector3 GetHMDRightGap()
        {
            // taken from OVRVision
            //return new Vector3(ImageProcessing.GetHMDRightGap(0) * 0.001f, ImageProcessing.GetHMDRightGap(1) * 0.001f, ImageProcessing.GetHMDRightGap(2) * 0.001f);	// 1/10
            return Vector3.zero;
        }

        public int SourceWidth
        {
            get { return ImageProcessing.GetCamWidth(); }
        }

        public int SourceHeight
        {
            get { return ImageProcessing.GetCamHeight(); }
        }

        public int SourceChannels
        {
            get { return ImageProcessing.GetCamChannels(); }
        }

        #endregion
    }
}
