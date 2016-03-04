using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Threading;
using UnityEngine;

namespace ComputerVision
{

    public class ImageProcessor
    {
        private static bool isInitialized = false;
        private static COvrvisionUnity ovrCamera = new COvrvisionUnity();

        // TODO: abstract settings?
        public static COvrvisionUnity Settings
        {
            get
            {
                return ovrCamera;
            }
        }

        private static bool keepRunning;

        public static void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogError("ImageProcessor was already initialized!");
                return;
            }

            bool camerasOpened = ovrCamera.Open(3, 0.15f);

            if (camerasOpened)
            {
                isInitialized = true;

                // TODO: userparams?
                ovrCamera.SetExposure(12960);
                ovrCamera.SetGain(47);
                ovrCamera.SetWhiteBalanceAutoMode(true);
                ovrCamera.SetBLC(0);
            }
            else
            {
                Debug.LogError("Could not open OVR cameras!");
            }
        }

        public static void Stop()
        {
            bool closeSuccess = ovrCamera.Close();
            keepRunning = false;

            if (!closeSuccess)
            {
                Debug.LogError("Could not close ovr cameras properly (?)");
            }
        }


        /// <summary>
        /// Writes the current image to the native unity texture pointers and
        /// starts computer vision modules on the current image.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        //public static unsafe void FetchCurrentImage(IntPtr left, IntPtr right, bool useImageProcessing)
        public static void FetchCurrentImage(IntPtr left, IntPtr right, bool useImageProcessing)
        {
            //Debug.Assert(isInitialized);
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            //if (!ovrCamera.camStatus)
            //{
            //    Debug.Log("Could not fetch image: Cameras not ready yet");
            //    return;
            //}

            //ovrCamera.UpdateImage(left, right);

            //if (useImageProcessing)
            //{
            //    var imWidth = ovrCamera.imageSizeW;
            //    var imHeight = ovrCamera.imageSizeH;

            //    var imgLeft = new Image<Bgra, byte>(new Size(imWidth, imHeight));
            //    var imgRight = new Image<Bgra, byte>(new Size(imWidth, imHeight));

            //    fixed (byte* leftPtr = imgLeft.Data)
            //    fixed (byte* rightPtr = imgRight.Data)
            //    {
            //        ovrCamera.UpdateImage2(new IntPtr(leftPtr), new IntPtr(rightPtr));
            //    }

            //    var thread = new Thread(() =>
            //    {
            //        ProcessImages(imgLeft, imgRight);
            //        imgLeft.Dispose();
            //        imgRight.Dispose();
            //    });

            //    thread.Start();
            //}

            //watch.Stop();
            //Debug.Log(String.Format("Took {0} ticks ({1} ms) {2} imageprocessing", watch.ElapsedTicks, watch.ElapsedMilliseconds, useImageProcessing ? "with" : "without"));
        }


        private static void ProcessImages(Image<Bgra, byte> imgLeft, Image<Bgra, byte> imgRight)
        {
            CvInvoke.Imshow("left", imgLeft);
            CvInvoke.Imshow("right", imgRight);
        }
    }
}
