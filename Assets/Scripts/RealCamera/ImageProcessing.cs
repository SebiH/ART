using System;
using System.Collections;
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
        private static extern float GetCamGain();

        [DllImport("ImageProcessing")]
        private static extern void SetCamGain(float val);

        [DllImport("ImageProcessing")]
        private static extern float GetCamExposure();

        [DllImport("ImageProcessing")]
        private static extern void SetCamExposure(float val);

        private delegate void DebugCallback(string message);
        [DllImport("ImageProcessing")]
        private static extern void RegisterDebugCallback(DebugCallback callback);

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

                // Issue a plugin event with arbitrary integer identifier.
                // The plugin can distinguish between different
                // things it needs to do based on this ID.
                // For our simple plugin, it does not matter which ID we pass here.
                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }



        private static List<Texture2D> syncHack = new List<Texture2D>();
        private static List<IntPtr> syncHack2 = new List<IntPtr>();
        public static void AddTextureSync(Texture2D tex)
        {
            syncHack.Add(tex);
        }

        void Update()
        {
            // quick hack to synchronize the rendering thread with this thread, to avoid crashes
            // TODO: better to write DLL as native rendering plugin
            syncHack2.Clear();
            foreach (var tex in syncHack)
                syncHack2.Add(tex.GetNativeTexturePtr());

            //UpdateTextures();
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
