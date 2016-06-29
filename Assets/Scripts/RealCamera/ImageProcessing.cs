using Assets.Code.Vision;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts.RealCamera
{

    public class ImageProcessing : MonoBehaviour
    {
        #region DllImports

        [DllImport("ImageProcessing")]
        private static extern void StartImageProcessing();

        // Deprecated for native rendering plugin
        //[DllImport("ImageProcessing")]
        //private static extern void UpdateTextures();

        [DllImport("ImageProcessing")]
        private static extern IntPtr GetRenderEventFunc();

        [DllImport("ImageProcessing")]
        private static extern int RegisterDx11TexturePtr(string moduleName, IntPtr texturePtr, int type);

        [DllImport("ImageProcessing")]
        private static extern void DeregisterTexturePtr(int handle);

        [DllImport("ImageProcessing")]
        private static extern int GetCamWidth();

        [DllImport("ImageProcessing")]
        private static extern int GetCamHeight();

        [DllImport("ImageProcessing")]
        private static extern int GetCamChannels();

        [DllImport("ImageProcessing")]
        private static extern int GetCamGain();

        [DllImport("ImageProcessing")]
        private static extern void SetCamGain(int val);

        [DllImport("ImageProcessing")]
        private static extern int GetCamExposure();

        [DllImport("ImageProcessing")]
        private static extern void SetCamExposure(int val);

        [DllImport("ImageProcessing")]
        private static extern int GetCamBLC();

        [DllImport("ImageProcessing")]
        private static extern void SetCamBLC(int val);

        [DllImport("ImageProcessing")]
        private static extern bool GetCamAutoWhiteBalance();

        [DllImport("ImageProcessing")]
        private static extern void SetCamAutoWhiteBalance(bool val);

        [DllImport("ImageProcessing")]
        private static extern int GetCamWhiteBalanceR();

        [DllImport("ImageProcessing")]
        private static extern void SetCamWhiteBalanceR(int val);

        [DllImport("ImageProcessing")]
        private static extern int GetCamWhiteBalanceG();

        [DllImport("ImageProcessing")]
        private static extern void SetCamWhiteBalanceG(int val);

        [DllImport("ImageProcessing")]
        private static extern int GetCamWhiteBalanceB();

        [DllImport("ImageProcessing")]
        private static extern void SetCamWhiteBalanceB(int val);

        private delegate void DebugCallback(string message);
        [DllImport("ImageProcessing")]
        private static extern void RegisterDebugCallback(DebugCallback callback);

        [DllImport("ImageProcessing")]
        private static extern void ChangeRoi(int moduleHandle, int x, int y, int width, int height);

        #endregion

        #region API


        public static int AddTexturePtr(Module module, IntPtr texturePtr, OutputType type)
        {
            return RegisterDx11TexturePtr(ModuleUtils.ModuleToString(module), texturePtr, (int)type);
        }

        public static void RemoveTexturePtr(int handle)
        {
            DeregisterTexturePtr(handle);
        }

        public static void ChangeRegionOfInterest(int x, int y, int width, int height)
        {
            ChangeRoi(-1, x, y, width, height);
        }

        #endregion


        IEnumerator Start()
        {
            RegisterDebugCallback(new DebugCallback(DebugMethod));
            yield return StartCoroutine("CallPluginAtEndOfFrames");
        }

        private static void DebugMethod(string message)
        {
            Debug.Log("[ImageProcessing]: " + message);
        }

        private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();

                UpdateCameraProperties();

                // Issue a plugin event with arbitrary integer identifier.
                // The plugin can distinguish between different
                // things it needs to do based on this ID.
                // For our simple plugin, it does not matter which ID we pass here.
                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }

        #region Camera Properties

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
            if (_prevGain != Gain || forceUpdate)
            {
                CameraGain = 8;
                _prevGain = Gain;
            }

            if (_prevExposure != Exposure || forceUpdate)
            {
                CameraExposure = _prevExposure;
                _prevExposure = Exposure;
            }
            
            if (_prevBLC != BLC || forceUpdate)
            {
                CameraBLC = BLC;
                _prevBLC = BLC;
            }

            if (_prevAutoWhiteBalance != AutoWhiteBalance || forceUpdate)
            {
                CameraAutoWhiteBalance = AutoWhiteBalance;
                _prevAutoWhiteBalance = AutoWhiteBalance;
            }

            if (_prevWhiteBalanceR != WhiteBalanceR || forceUpdate)
            {
                CameraWhiteBalanceR = WhiteBalanceR;
                _prevWhiteBalanceR = WhiteBalanceR;
            }

            if (_prevWhiteBalanceG != WhiteBalanceG || forceUpdate)
            {
                CameraWhiteBalanceG = WhiteBalanceG;
                _prevWhiteBalanceG = WhiteBalanceG;
            }

            if (_prevWhiteBalanceB != WhiteBalanceB || forceUpdate)
            {
                CameraWhiteBalanceB = WhiteBalanceB;
                _prevWhiteBalanceB = WhiteBalanceB;
            }
        }


        public static int CameraWidth
        {
            get { return GetCamWidth(); }
        }

        public static int CameraHeight
        {
            get { return GetCamHeight(); }
        }

        public static int CameraChannels
        {
            get { return GetCamChannels(); }
        }

        public static int CameraGain
        {
            get { return GetCamGain(); }
            set { SetCamGain(value); }
        }

        public static int CameraExposure
        {
            get { return GetCamExposure(); }
            set { SetCamExposure(value); }
        }

        public static int CameraBLC
        {
            get { return GetCamBLC(); }
            set { SetCamBLC(value); }
        }

        public static bool CameraAutoWhiteBalance
        {
            get { return GetCamAutoWhiteBalance(); }
            set { SetCamAutoWhiteBalance(value); }
        }

        public static int CameraWhiteBalanceR
        {
            get { return GetCamWhiteBalanceR(); }
            set { SetCamWhiteBalanceR(value); }
        }

        public static int CameraWhiteBalanceG
        {
            get { return GetCamWhiteBalanceG(); }
            set { SetCamWhiteBalanceG(value); }
        }

        public static int CameraWhiteBalanceB
        {
            get { return GetCamWhiteBalanceB(); }
            set { SetCamWhiteBalanceB(value); }
        }

        #endregion

    }
}
