using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{


    public static class ImageProcessor
    {
        [DllImport("ImageProcessing")]
        private static extern void DetectMarker([MarshalAs(UnmanagedType.Struct)] ref MarshalledImageData image, [MarshalAs(UnmanagedType.Struct)] ref MarshalledPose pose);


        [DllImport("ImageProcessing")]
        private static extern void RunOvrTest();




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



        //[DllImport("ImageProcessing")]
        //private static extern void ProcessOvr();

        public static unsafe void TrackMarker(ref Image<Bgr, byte> image, ref MarshalledPose pose)
        {
            // TODO: possible optimisation when handling images, mats in emgucv:
            // http://www.emgu.com/wiki/index.php/Working_with_Images#Creating_Image
            MarshalledImageData unmanagedImage = new MarshalledImageData
            {
                height = image.Height,
                width = image.Width,
                channels = image.NumberOfChannels
            };

            fixed (byte* imgPtr = image.Data)
            {
                unmanagedImage.rawData = imgPtr;
                DetectMarker(ref unmanagedImage, ref pose);
            }
        }


        public const int OV_CAMEYE_LEFT = 0;
        public const int OV_CAMEYE_RIGHT = 1;

        public static unsafe void TestOvr()
        {
            // open camera
            if (ovOpen(0, 0.15f, 3) != 0)
            {
                return;
            }

            ovSetExposure(12960);
            ovSetGain(47);
            ovSetWhiteBalanceAuto(true);
            ovSetCamSyncMode(false);
            ovSetBLC(0);

            var camWidth = ovGetImageWidth();
            var camHeight = ovGetImageHeight();

            using (var imgLeft = new Image<Bgra, byte>(new Size(camWidth, camHeight)))
            using (var imgRight = new Image<Bgra, byte>(new Size(camWidth, camHeight)))
            using (var imgComposite = new Image<Bgra, byte>(new Size(camWidth * 2, camHeight)))
            {
                while (true)
                {
                    ovPreStoreCamData(1);

                    fixed (byte* imgLeftPtr = imgLeft.Data)
                    fixed (byte* imgRightPtr = imgRight.Data)
                    {
                        ovGetCamImageBGRA(new IntPtr(imgLeftPtr), OV_CAMEYE_LEFT);
                        ovGetCamImageBGRA(new IntPtr(imgRightPtr), OV_CAMEYE_RIGHT);
                    }

                    var roiLeft = new Mat(imgComposite.Mat, new Rectangle(0, 0, camWidth, camHeight));
                    imgLeft.Mat.CopyTo(roiLeft);

                    var roiRight = new Mat(imgComposite.Mat, new Rectangle(camWidth, 0, camWidth, camHeight));
                    imgRight.Mat.CopyTo(roiRight);

                    CvInvoke.Imshow("Composite", imgComposite);
                    CvInvoke.WaitKey(10);
                }
            }
        }
    }
}
