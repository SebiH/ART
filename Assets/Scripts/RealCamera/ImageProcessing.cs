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
        private static extern float GetCameraProperty(string propName);

        [DllImport("ImageProcessing")]
        private static extern void SetCameraProperty(string propName, float propVal);

        [DllImport("ImageProcessing")]
        private static extern void UpdateTextures();

        [DllImport("ImageProcessing")]
        private static extern int RegisterDx11TexturePtr(string moduleName, IntPtr texturePtr, int type);

        [DllImport("ImageProcessing")]
        private static extern void DeregisterTexturePtr(int handle);


        #endregion

        #region API

        public static void StartProcessing()
        {
            StartImageProcessing();
        }

        public static float GetCamProperty(string prop)
        {
            return GetCameraProperty(prop);
        }

        public static void SetCamProperty(string prop, float val)
        {
            SetCameraProperty(prop, val);
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

    }
}
