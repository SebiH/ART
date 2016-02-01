using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageProcessingUtil
{


    public static class ImageProcessor
    {
        [DllImport("ImageProcessing")]
        private static extern void DetectMarker([MarshalAs(UnmanagedType.Struct)] ref MarshalledImageData image, [MarshalAs(UnmanagedType.Struct)] ref MarshalledPose pose);





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

        public static void TestOvr()
        {
            ovSetExposure(12960);
            ovSetGain(47);
            ovSetWhiteBalanceAuto(true);
            ovSetCamSyncMode(false);
            ovSetBLC(0);

            int opentype = 3;
            int imageSizeW, imageSizeH;
            Bitmap imageDataLeft, imageDataRight;

            //Open camera
            if (ovOpen(0, 0.15f, opentype) == 0)
            {
                imageSizeW = ovGetImageWidth();
                imageSizeH = ovGetImageHeight();

                //Create bitmap
                imageDataLeft = new Bitmap(imageSizeW, imageSizeH, PixelFormat.Format24bppRgb);
                imageDataRight = new Bitmap(imageSizeW, imageSizeH, PixelFormat.Format24bppRgb);
            }
            else
            {
                return;
            }


            while (true)
            {
                ovPreStoreCamData(1);

                // get image data
                BitmapData dataLeft = imageDataLeft.LockBits(new Rectangle(0, 0, imageSizeW, imageSizeH), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                ovGetCamImageBGR(dataLeft.Scan0, OV_CAMEYE_LEFT);
                imageDataLeft.UnlockBits(dataLeft);

                BitmapData dataRight = imageDataRight.LockBits(new Rectangle(0, 0, imageSizeW, imageSizeH), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                ovGetCamImageBGR(dataRight.Scan0, OV_CAMEYE_RIGHT);
                imageDataRight.UnlockBits(dataRight);

                // convert to emgucv
                using (var imgLeft = new Image<Bgra, byte>(imageDataLeft))
                using (var imgRight = new Image<Bgra, byte>(imageDataRight))
                {
                    CvInvoke.Imshow("left", imgLeft);
                    CvInvoke.Imshow("right", imgRight);
                    CvInvoke.WaitKey();
                }
            }
        }
    }
}
