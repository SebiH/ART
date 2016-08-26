using System;
using System.Runtime.InteropServices;

namespace ImageProcessingTest
{
    public static class ImageProcessing
    {
        #region camera_factory

        [DllImport("ImageProcessing")]
        public static extern void StartImageProcessing();

        [DllImport("ImageProcessing")]
        public static extern void StopImageProcessing();

        [DllImport("ImageProcessing")]
        public static extern void SetOvrCamera(int resolution, int processing_mode);

        [DllImport("ImageProcessing")]
        public static extern void SetDummyCamera(string filepath);

        #endregion

        #region camera_properties

        [DllImport("ImageProcessing")]
        public static extern int GetCamWidth();

        [DllImport("ImageProcessing")]
        public static extern int GetCamHeight();

        [DllImport("ImageProcessing")]
        public static extern int GetCamChannels();

        [DllImport("ImageProcessing")]
        public static extern int GetCamGain();

        [DllImport("ImageProcessing")]
        public static extern void SetCamGain(int val);

        [DllImport("ImageProcessing")]
        public static extern int GetCamExposure();

        [DllImport("ImageProcessing")]
        public static extern void SetCamExposure(int val);

        [DllImport("ImageProcessing")]
        public static extern void SetCamExposurePerSec(float val);

        [DllImport("ImageProcessing")]
        public static extern int GetCamBLC();

        [DllImport("ImageProcessing")]
        public static extern void SetCamBLC(int val);

        [DllImport("ImageProcessing")]
        public static extern bool GetCamAutoWhiteBalance();

        [DllImport("ImageProcessing")]
        public static extern void SetCamAutoWhiteBalance(bool val);

        [DllImport("ImageProcessing")]
        public static extern int GetCamWhiteBalanceR();

        [DllImport("ImageProcessing")]
        public static extern void SetCamWhiteBalanceR(int val);

        [DllImport("ImageProcessing")]
        public static extern int GetCamWhiteBalanceG();

        [DllImport("ImageProcessing")]
        public static extern void SetCamWhiteBalanceG(int val);

        [DllImport("ImageProcessing")]
        public static extern int GetCamWhiteBalanceB();

        [DllImport("ImageProcessing")]
        public static extern void SetCamWhiteBalanceB(int val);

        [DllImport("ImageProcessing")]
        public static extern int GetCamFps();

        [DllImport("ImageProcessing")]
        public static extern float GetHMDRightGap(int at);

        [DllImport("ImageProcessing")]
        public static extern float GetCamFocalPoint();

        [DllImport("ImageProcessing")]
        public static extern int GetProcessingMode();

        [DllImport("ImageProcessing")]
        public static extern void SetProcessingMode(int mode);

        #endregion

        #region logging

        public delegate void DebugCallback(string message);

        [DllImport("ImageProcessing")]
        public static extern void RegisterLoggerCallback(DebugCallback callback);

        #endregion

        #region output_factory

        [DllImport("ImageProcessing")]
        public static extern int AddOpenCvOutput(int pipeline_id, string windowname);

        [DllImport("ImageProcessing")]
        public static extern int OpenCvWaitKey(int delay);

        [DllImport("ImageProcessing")]
        public static extern int RegisterUnityPointer(int pipeline_id, int eye, IntPtr texture_ptr);

        [DllImport("ImageProcessing")]
        public static extern int RemoveOutput(int pipeline_id, int output_id);

        #endregion

        #region pipeline

        [DllImport("ImageProcessing")]
        public static extern int CreatePipeline();

        [DllImport("ImageProcessing")]
        public static extern void RemovePipeline(int id);

        #endregion

        #region processor_factory

        [DllImport("ImageProcessing")]
        public static extern int AddArToolkitProcessor(int pipeline_id);

        #endregion

        #region unity_plugin

        [DllImport("ImageProcessing")]
        public static extern IntPtr GetRenderEventFunc();

        #endregion
    }
}
