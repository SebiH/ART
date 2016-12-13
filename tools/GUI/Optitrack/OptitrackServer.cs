using System.Runtime.InteropServices;

namespace GUI
{
    public static class OptitrackServer
    {
        public delegate void LoggerCallback(string message);

        [DllImport("optitrack")]
        public static extern void SetLogger(int LogLevel, LoggerCallback callback);

        [DllImport("optitrack")]
        public static extern bool StartOptitrackServer(string OptitrackIp, int DataPort, int CommandPort, string ListenIp);

        [DllImport("optitrack")]
        public static extern void StopOptitrackServer();

        [DllImport("optitrack")]
        public static extern void AttachUnityOutput(string UnityIp, int port);
    }
}
