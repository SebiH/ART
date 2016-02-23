using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Assets.Code
{
    static class CameraImageProvider
    {
        #region OVRVision DLL Import
        //Ovrvision Dll import
        //ovrvision_csharp.cpp
        ////////////// Main Ovrvision System //////////////
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovOpen(int locationID, float arMeter, int type);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovClose();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovRelease();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovPreStoreCamData(int qt);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovGetCamImageBGRA(System.IntPtr img, int eye);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovGetCamImageRGB(System.IntPtr img, int eye);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovGetCamImageBGR(System.IntPtr img, int eye);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovGetCamImageForUnity(System.IntPtr pImagePtr_Left, System.IntPtr pImagePtr_Right);

        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovGetCamImageForUnityNative(System.IntPtr pTexPtr_Left, System.IntPtr pTexPtr_Right);

        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetImageWidth();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetImageHeight();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetImageRate();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetBufferSize();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetPixelSize();

        //Set camera properties
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovSetExposure(int value);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovSetGain(int value);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovSetBLC(int value);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovSetWhiteBalanceR(int value);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovSetWhiteBalanceG(int value);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovSetWhiteBalanceB(int value);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovSetWhiteBalanceAuto(bool value);
        //Get camera properties
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetExposure();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetGain();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetBLC();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetWhiteBalanceR();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetWhiteBalanceG();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetWhiteBalanceB();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern bool ovGetWhiteBalanceAuto();

        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern float ovGetFocalPoint();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern float ovGetHMDRightGap(int at);

        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern float ovSetCamSyncMode(bool at);

        ////////////// Ovrvision AR System //////////////
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovARRender();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovARGetData(System.IntPtr mdata, int datasize);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovARSetMarkerSize(int value);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovARGetMarkerSize();

        ////////////// Ovrvision Tracking System //////////////
        //testing
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovTrackRender(bool calib, bool point);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovGetTrackData(System.IntPtr mdata);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovTrackingCalibReset();

        ////////////// Ovrvision Calibration //////////////
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovCalibInitialize(int pattern_size_w, int pattern_size_h, double chessSizeMM);
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovCalibClose();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovCalibFindChess();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void ovCalibSolveStereoParameter();
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int ovCalibGetImageCount();

        //Ovrvision config save status
        [DllImport("ovrvision", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern bool ovSaveCamStatusToEEPROM();
        #endregion

        public const int OV_CAMEYE_LEFT = 0;
        public const int OV_CAMEYE_RIGHT = 1;

        private static bool isRunning;
        private static bool keepRunning;

        public static bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        public static void Start()
        {
            if (!isRunning)
            {
                isRunning = true;
                keepRunning = true;
                Thread t = new Thread(new ThreadStart(Run));
                t.Start();
            }
        }

        public static void Stop()
        {
            keepRunning = false;
        }



        private static void Run()
        {
            // open camera
            if (ovOpen(0, 0.15f, 3) != 0)
            {
                Debug.LogError("Could not open OVRVision Cameras");
                isRunning = false;
                return;
            }

            ovSetExposure(12960);
            ovSetGain(47);
            ovSetWhiteBalanceAuto(true);
            ovSetCamSyncMode(false);
            ovSetBLC(0);

            try
            {
                FetchCameraImage();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            ovClose();
            isRunning = false;
        }

        private static unsafe void FetchCameraImage()
        {
            var camWidth = ovGetImageWidth();
            var camHeight = ovGetImageHeight();

            using (var imgLeft = new Image<Bgra, byte>(new Size(camWidth, camHeight)))
            using (var imgRight = new Image<Bgra, byte>(new Size(camWidth, camHeight)))
            {
                while (keepRunning)
                {
                    ovPreStoreCamData(1);

                    fixed (byte* imgLeftPtr = imgLeft.Data)
                    fixed (byte* imgRightPtr = imgRight.Data)
                    {
                        ovGetCamImageBGRA(new IntPtr(imgLeftPtr), OV_CAMEYE_LEFT);
                        ovGetCamImageBGRA(new IntPtr(imgRightPtr), OV_CAMEYE_RIGHT);
                    }

                    leftRawImage = imgLeft.Bytes;
                    rightRawImage = imgRight.Bytes;
                    currentImageId++;
                }
            }
        }


        private static byte[] leftRawImage;
        private static byte[] rightRawImage;
        private static int currentImageId;

        public static byte[] GetLeftRawImage()
        {
            return leftRawImage;
        }

        public static byte[] GetRightRawImage()
        {
            return rightRawImage;
        }

        public static int GetCurrentImageId()
        {
            return currentImageId;
        }

    }
}
