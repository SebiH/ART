using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts.RealCamera
{

    public class ImageProcessing : MonoBehaviour
    {
        public static readonly String MODULE_RAW_IMAGE = "RawImage";
        public static readonly String MODULE_ROI = "ROI";

        #region DllImports

        [DllImport("ImageProcessing")]
        private static extern void StartImageProcessing();

        [DllImport("ImageProcessing")]
        private static extern void UpdateTextures();

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
        private static extern float GetCamGain();

        [DllImport("ImageProcessing")]
        private static extern void SetCamGain(float val);

        [DllImport("ImageProcessing")]
        private static extern float GetCamExposure();

        [DllImport("ImageProcessing")]
        private static extern void SetCamExposure(float val);

        #endregion

        #region API

        public static void StartProcessing()
        {
            StartImageProcessing();
        }

        public enum Type { left = 0, right = 1, combined = 2 };

        public static int AddTexturePtr(string moduleName, IntPtr texturePtr, Type type)
        {
            return RegisterDx11TexturePtr(moduleName, texturePtr, (int)type);
        }

        public static void RemoveTexturePtr(int handle)
        {
            DeregisterTexturePtr(handle);
        }

        #endregion

        void Update()
        {
            UpdateTextures();
        }


        // Camera properties

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

        public static float CameraGain
        {
            get { return GetCamGain(); }
            set { SetCamGain(value); }
        }

        public static float CameraExposure
        {
            get { return GetCamExposure(); }
            set { SetCamExposure(value); }
        }
    }
}
