using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Demo
{
    public class OvrHandler : MonoBehaviour
    {
        public static OvrHandler Instance
        {
            get; private set;
        }

        public readonly COvrvisionUnity OvrPro = new COvrvisionUnity();

        public enum CameraMode
        {
            //2560x1920 @15fps x2
            OV_CAM5MP_FULL = 0,

            //1920x1080 @30fps x2
            OV_CAM5MP_FHD = 1,

            //1280x960  @45fps x2
            OV_CAMHD_FULL = 2,

            //960x950   @60fps x2
            OV_CAMVR_FULL = 3,

            //1280x800  @60fps x2
            OV_CAMVR_WIDE = 4,

            //640x480   @90fps x2
            OV_CAMVR_VGA = 5,

            //320x240   @120fps x2
            OV_CAMVR_QVGA = 6
        };

        public CameraMode cameraMode = CameraMode.OV_CAMVR_WIDE;

        void Awake()
        {
            Instance = this;

            if (OvrPro.Open((int)cameraMode, 0.15f))
            {
                UpdateCameraProperties(true);
                Thread.Sleep(100);
            }
            else
            {
                Debug.LogError("Unable to open OVRVision Cameras");
            }
        }

        public IntPtr LeftTexturePtr;
        public IntPtr RightTexturePtr;

        void Update()
        {
            if (!OvrPro.camStatus)
            {
                return;
            }

            UpdateCameraProperties(false);

            if (LeftTexturePtr.ToInt64() != 0 && RightTexturePtr.ToInt64() != 0)
            {
                OvrPro.UpdateImage(LeftTexturePtr, RightTexturePtr);
            }

            if (LeftTexturePtr == null)
            {
                Debug.LogError("No LeftTexture specified! OVRVision needs both!");
            }

            if (RightTexturePtr == null)
            {
                Debug.LogError("No RightTexture specified! OVRVision needs both!");
            }
        }

        void OnDestroy()
        {
            if (!OvrPro.Close())
            {
                Debug.LogError("Could not close OVRVision cameras");
            }
        }



        #region CameraProperties

        [Range(1, 47)]
        public int Gain = 8;
        private int _prevGain;

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

        private void UpdateCameraProperties(bool forceUpdate = false)
        {
            if (!OvrPro.camStatus)
            {
                return;
            }

            if (_prevGain != Gain || forceUpdate)
            {
                OvrPro.SetGain(Gain);
                _prevGain = Gain;
            }

            if (_prevExposure != Exposure || forceUpdate)
            {
                OvrPro.SetExposure(Exposure);
                _prevExposure = Exposure;
            }
            
            if (_prevBLC != BLC || forceUpdate)
            {
                OvrPro.SetBLC(BLC);
                _prevBLC = BLC;
            }

            if (_prevAutoWhiteBalance != AutoWhiteBalance || forceUpdate)
            {
                OvrPro.SetWhiteBalanceAutoMode(AutoWhiteBalance);
                _prevAutoWhiteBalance = AutoWhiteBalance;
            }

            if (_prevWhiteBalanceR != WhiteBalanceR || forceUpdate)
            {
                OvrPro.SetWhiteBalanceR(WhiteBalanceR);
                _prevWhiteBalanceR = WhiteBalanceR;
            }

            if (_prevWhiteBalanceG != WhiteBalanceG || forceUpdate)
            {
                OvrPro.SetWhiteBalanceG(WhiteBalanceG);
                _prevWhiteBalanceG = WhiteBalanceG;
            }

            if (_prevWhiteBalanceB != WhiteBalanceB || forceUpdate)
            {
                OvrPro.SetWhiteBalanceG(WhiteBalanceG);
                _prevWhiteBalanceB = WhiteBalanceB;
            }
        }

        #endregion
    }
}
