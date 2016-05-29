using Assets.Code.Util;
using System;
using UnityEngine;

namespace Assets.Scripts.RealCamera
{
    class OvrCameraWindow : MonoBehaviour
    {
        public int ImageWidth = 500;
        public int ImageHeight = 500;
        public int StartPosX = 0;
        public int StartPosY = 0;

        public GameObject CameraPlaneLeft;
        public GameObject CameraPlaneRight;

        // max height depending on ovr camera feed - can't be more than is available!
        private int MaxImageWidth;
        private int MaxImageHeight;

        private int TextureHandleLeft = -1;
        private int TextureHandleRight = -1;

        void Awake()
        {
            MaxImageWidth = ImageProcessing.CameraWidth;
            MaxImageHeight = ImageProcessing.CameraHeight;
        }


        void Start()
        {
            CameraPlaneLeft.layer = Layers.LEFT_EYE_ONLY;
            CameraPlaneRight.layer = Layers.RIGHT_EYE_ONLY;
        }

        private int _prevImageWidth = -1;
        private int _prevImageHeight = -1;
        private int _prevStartPosX = -1;
        private int _prevStartPosY = -1;

        void Update()
        {
            var imgWidth = Math.Min(Math.Max(10, ImageWidth), MaxImageWidth - 1);
            var imgHeight = Math.Min(Math.Max(10, ImageHeight), MaxImageHeight - 1);
            var posX = Math.Min(Math.Max(0, StartPosX), MaxImageWidth - imgWidth);
            var posY = Math.Min(Math.Max(0, StartPosY), MaxImageHeight - imgHeight);

            var hasSizeChanged = (_prevImageHeight != imgHeight || _prevImageWidth != imgWidth);
            var hasPositionChanged = (_prevStartPosX != posX || _prevStartPosY != posY);

            if (hasSizeChanged)
            {
                _prevImageHeight = imgHeight;
                _prevImageWidth = imgWidth;

                DeregisterTextures();

                // TODO: needs refactoring..
                var textures = CreateTextures(imgWidth, imgHeight);
                RegisterTextures(textures[0], textures[1]);
            }


            if (hasPositionChanged || hasSizeChanged)
            {
                _prevStartPosX = posX;
                _prevStartPosY = posY;
                ImageProcessing.ChangeRegionOfInterest(posX, posY, imgWidth, imgHeight);
            }
        }

        // TODO: needs refactoring, side-effects!
        private Texture2D[] CreateTextures(int width, int height)
        {
            // Create cam texture
            TextureFormat tf = (ImageProcessing.CameraChannels == 3) ? TextureFormat.RGB24 : TextureFormat.BGRA32;
            var CameraTexLeft = new Texture2D(width, height, tf, false);
            var CameraTexRight = new Texture2D(width, height, tf, false);

            // Cam setting
            CameraTexLeft.wrapMode = TextureWrapMode.Clamp;
            CameraTexRight.wrapMode = TextureWrapMode.Clamp;

            CameraPlaneLeft.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexLeft);
            CameraPlaneRight.GetComponent<Renderer>().materials[0].SetTexture("_MainTex", CameraTexRight);

            return new[] { CameraTexLeft, CameraTexRight };
        }

        // TODO: needs refactoring
        private void RegisterTextures(Texture2D leftTexture, Texture2D rightTexture)
        {
            if (leftTexture != null)
            {
                var leftTexturePtr = leftTexture.GetNativeTexturePtr();
                ImageProcessing.AddTexturePtr(ImageProcessing.MODULE_ROI, leftTexturePtr, ImageProcessing.Type.left);
            }

            if (rightTexture != null)
            {
                var rightTexturePtr = rightTexture.GetNativeTexturePtr();
                ImageProcessing.AddTexturePtr(ImageProcessing.MODULE_ROI, rightTexturePtr, ImageProcessing.Type.right);
            }
        }

        private void DeregisterTextures()
        {
            if (TextureHandleLeft != -1)
            {
                ImageProcessing.RemoveTexturePtr(TextureHandleLeft);
                TextureHandleLeft = -1;
            }

            if (TextureHandleRight != -1)
            {
                ImageProcessing.RemoveTexturePtr(TextureHandleRight);
                TextureHandleRight = -1;
            }
        }


        void OnDestroy()
        {
            DeregisterTextures();
        }

    }
}
