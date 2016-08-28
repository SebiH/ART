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

        void Update()
        {
            if (_isRunning)
            {
                UpdateCameraProperties();
            }
        }


        #region Camera Properties

        public int Gain = 8;
        private int _prevGain;

        public int Exposure = 12960;
        private int _prevExposure;

        public int BLC = 32;
        private int _prevBLC;

        public bool AutoWhiteBalance = true;
        private bool _prevAutoWhiteBalance;

        public int WhiteBalanceR = 1474;
        private int _prevWhiteBalanceR;

        public int WhiteBalanceG = 1024;
        private int _prevWhiteBalanceG;

        public int WhiteBalanceB = 1738;
        private int _prevWhiteBalanceB;

        private void LoadCurrentProperties()
        {
            _prevGain = SourceGain;
            _prevExposure = SourceExposure;
            _prevBLC = SourceBLC;

            _prevAutoWhiteBalance = SourceAutoWhiteBalance;
            _prevWhiteBalanceR = SourceWhiteBalanceR;
            _prevWhiteBalanceG = SourceWhiteBalanceG;
            _prevWhiteBalanceB = SourceWhiteBalanceB;
        }

        public void UpdateCameraProperties(bool forceUpdate = false)
        {
            if (_prevGain != Gain || forceUpdate)
            {
                SourceGain = Gain;
                _prevGain = Gain;
            }

            if (_prevExposure != Exposure || forceUpdate)
            {
                SourceExposure = _prevExposure;
                _prevExposure = Exposure;
            }

            if (_prevBLC != BLC || forceUpdate)
            {
                SourceBLC = BLC;
                _prevBLC = BLC;
            }

            if (_prevAutoWhiteBalance != AutoWhiteBalance || forceUpdate)
            {
                SourceAutoWhiteBalance = AutoWhiteBalance;
                _prevAutoWhiteBalance = AutoWhiteBalance;
            }

            if (_prevWhiteBalanceR != WhiteBalanceR || forceUpdate)
            {
                SourceWhiteBalanceR = WhiteBalanceR;
                _prevWhiteBalanceR = WhiteBalanceR;
            }

            if (_prevWhiteBalanceG != WhiteBalanceG || forceUpdate)
            {
                SourceWhiteBalanceG = WhiteBalanceG;
                _prevWhiteBalanceG = WhiteBalanceG;
            }

            if (_prevWhiteBalanceB != WhiteBalanceB || forceUpdate)
            {
                SourceWhiteBalanceB = WhiteBalanceB;
                _prevWhiteBalanceB = WhiteBalanceB;
            }
        }


        public Vector3 GetHMDRightGap()
        {
            // taken from OVRVision
            return new Vector3(ImageProcessing.GetHMDRightGap(0) * 0.001f, ImageProcessing.GetHMDRightGap(1) * 0.001f, ImageProcessing.GetHMDRightGap(2) * 0.001f);	// 1/10
        }

        public int SourceWidth
        {
            get { return ImageProcessing.GetCamWidth(); }
        }

        public int SourceHeight
        {
            get { return ImageProcessing.GetCamHeight(); }
        }

        public int SourceFps
        {
            get { return ImageProcessing.GetCamFps(); }
        }

        public int SourceChannels
        {
            get { return ImageProcessing.GetCamChannels(); }
        }

        private int SourceGain
        {
            get { return ImageProcessing.GetCamGain(); }
            set { ImageProcessing.SetCamGain(value); }
        }

        private int SourceExposure
        {
            get { return ImageProcessing.GetCamExposure(); }
            set { ImageProcessing.SetCamExposure(value); }
        }

        private int SourceBLC
        {
            get { return ImageProcessing.GetCamBLC(); }
            set { ImageProcessing.SetCamBLC(value); }
        }

        private bool SourceAutoWhiteBalance
        {
            get { return ImageProcessing.GetCamAutoWhiteBalance(); }
            set { ImageProcessing.SetCamAutoWhiteBalance(value); }
        }

        private int SourceWhiteBalanceR
        {
            get { return ImageProcessing.GetCamWhiteBalanceR(); }
            set { ImageProcessing.SetCamWhiteBalanceR(value); }
        }

        private int SourceWhiteBalanceG
        {
            get { return ImageProcessing.GetCamWhiteBalanceG(); }
            set { ImageProcessing.SetCamWhiteBalanceG(value); }
        }

        private int SourceWhiteBalanceB
        {
            get { return ImageProcessing.GetCamWhiteBalanceB(); }
            set { ImageProcessing.SetCamWhiteBalanceB(value); }
        }

        public float SourceFocalPoint
        {
            get
            {
                // taken from OVRVision
                return ImageProcessing.GetCamFocalPoint() * 0.001f; // 1/100
            }
        }

        #endregion
    }
}
