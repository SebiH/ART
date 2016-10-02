using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public static class OptitrackServer
    {
        public delegate void OutputCallback(string message);

        [DllImport("optitrack")]
        public static extern void RegisterCallback(OutputCallback callback);

        [DllImport("optitrack")]
        public static extern void ReplayFromData(string filename, int loglevel);

        [DllImport("optitrack")]
        public static extern void StartServer(string OptitrackIp, string ListenIp, string UnityIp, string SaveFile, int LogLevel);

        [DllImport("optitrack")]
        public static extern void StopServer();
    }
}
