using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts.RealCamera
{

    public class ImageProcessing : MonoBehaviour
    {
        public enum ImageProcessingMethod
        {
            Native,
            ROI
        }

        #region DllImports

        [DllImport("ImageProcessing")]
        private static extern void OvrStart(int cameraMode = -1);

        [DllImport("ImageProcessing")]
        private static extern void OvrStop();

        [DllImport("ImageProcessing")]
        private static extern void WriteROITexture(int startX, int startY, int width, int height, IntPtr leftUnityPtr, IntPtr rightUnityPtr);

        [DllImport("ImageProcessing")]
        private static extern float GetProperty(string prop);

        [DllImport("ImageProcessing")]
        private static extern void SetProperty(string prop, float val);

        [DllImport("ImageProcessing")]
        private static extern void WriteTexture(IntPtr leftUnityPtr, IntPtr rightUnityPtr);

        [DllImport("ImageProcessing")]
        private static extern void FetchImage();

        [DllImport("ImageProcessing")]
        private static extern void RegisterExperimentalTexturePtr(IntPtr ptr);

        [DllImport("ImageProcessing")]
        private static extern void UpdateExperimentalTexturePtr();

        #endregion

        #region Singleton

        // Singleton ..if possible
        public static ImageProcessing Instance;

        public ImageProcessing() : base()
        {
            if (Instance != null)
            {
                Debug.LogError("There should only be one instance of ImageProcessing");
            }
            else
            {
                Instance = this;
            }
        }

        #endregion

        #region API

        private bool _hasStarted = false;
        private int _subscriberCount = 0;
        public void RequestStart(int cameraMode = -1)
        {
            _subscriberCount++;
            if (!_hasStarted)
            {
                OvrStart(cameraMode);
            }
        }


        public void RequestShutdown()
        {
            _subscriberCount--;

            if (_subscriberCount <= 0)
            {
                OvrStop();
            }

            if (_subscriberCount < 0)
            {
                Debug.LogError("SubscriberCount should not be negative!");
            }
        }


        public float GetCameraProperty(string prop)
        {
            return GetProperty(prop);
        }

        public void SetCameraProperty(string prop, float val)
        {
            SetProperty(prop, val);
        }


        #endregion

        #region Updating textures

        struct TextureRequest
        {
            public ImageProcessingMethod method;
            public IntPtr leftPtr;
            public IntPtr rightPtr;
            public object[] args;
        }

        private List<TextureRequest> _textureUpdateRequests = new List<TextureRequest>();

        public void RegisterTextureUpdate(ImageProcessingMethod method, IntPtr leftPtr, IntPtr rightPtr, params object[] args)
        {
            _textureUpdateRequests.Add(new TextureRequest
            {
                method = method,
                leftPtr = leftPtr,
                rightPtr = rightPtr,
                args = args
            });
        }

        public void AddExperimentalTexturePtr(IntPtr texture)
        {
            RegisterExperimentalTexturePtr(texture);
        }


        public void DeregisterTexture(IntPtr leftPtr, IntPtr rightPtr)
        {
            _textureUpdateRequests.RemoveAll((req) =>
            {
                return req.leftPtr == leftPtr && req.rightPtr == rightPtr;
            });
        }



        void Update()
        {
            FetchImage();

            foreach (var req in _textureUpdateRequests)
            {
                switch (req.method)
                {
                    case ImageProcessingMethod.Native:
                        WriteTexture(req.leftPtr, req.rightPtr);
                        break;

                    case ImageProcessingMethod.ROI:
                        WriteROITexture((int)req.args[0], (int)req.args[1], (int)req.args[2], (int)req.args[3], req.leftPtr, req.rightPtr);
                        break;

                    default:
                        Debug.LogError("Unknown ImageProcessingMethod");
                        break;
                }
            }

            UpdateExperimentalTexturePtr();

        }

        #endregion
    }
}
