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
        private static extern void RegisterDx11TexturePtr(string moduleName, int texturePtrCount, IntPtr[] texturePtr);

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


        public static void AddTexturePtrs(string moduleName, IntPtr[] texturePtrs)
        {
            RegisterDx11TexturePtr(moduleName, texturePtrs.Length, texturePtrs);
        }

        #endregion

        void Update()
        {
            UpdateTextures();
        }

    }
}
