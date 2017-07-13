using System;
using System.Runtime.InteropServices;

namespace Assets.Modules.Vision
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
        public static extern void SetFileCamera(string filepath);

        [DllImport("ImageProcessing")]
        public static extern void SetOpenCVCamera();

        [DllImport("ImageProcessing")]
        public static extern void SetEmptyCamera();

        #endregion

        #region camera_properties

        [DllImport("ImageProcessing")]
        public static extern int GetCamWidth();

        [DllImport("ImageProcessing")]
        public static extern int GetCamHeight();

        [DllImport("ImageProcessing")]
        public static extern int GetCamChannels();

        public delegate void PropertyCallback(string json_properties);
        [DllImport("ImageProcessing")]
        public static extern void GetCamJsonProperties(PropertyCallback callback);

        [DllImport("ImageProcessing")]
        public static extern void SetCamJsonProperties(string json_str_config);

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

        public delegate void JsonCallback(string payload);

        [DllImport("ImageProcessing")]
        public static extern int AddJsonOutput(int pipeline_id, JsonCallback callback);

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
        public static extern int AddArToolkitStereoProcessor(int pipeline_id, string json_config);


        [DllImport("ImageProcessing")]
        public static extern int AddArToolkitProcessor(int pipeline_id, string json_config);

        [DllImport("ImageProcessing")]
        public static extern int AddArucoProcessor(int pipeline_id, string json_config);

        [DllImport("ImageProcessing")]
        public static extern int AddArucoMapProcessor(int pipeline_id, string json_config);

        [DllImport("ImageProcessing")]
        public static extern int AddUndistortProcessor(int pipeline_id, string json_config);

        [DllImport("ImageProcessing")]
        public static extern int RemoveProcessor(int pipeline_id, int processor_id);

        public delegate void ProcessorPropertyCallback(string json_properties);
        [DllImport("ImageProcessing")]
        public static extern void GetProcessorProperties(int pipeline_id, int processor_id, ProcessorPropertyCallback callback);

        [DllImport("ImageProcessing")]
        public static extern void SetProcessorProperties(int pipeline_id, int processor_id, string json_config_str);

        #endregion

        #region unity_plugin

        [DllImport("ImageProcessing")]
        public static extern IntPtr GetRenderEventFunc();

        #endregion
    }

}
