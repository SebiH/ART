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
        [DllImport("optitrack")]
        public static extern void ReplayFromData(string filename);

        [DllImport("optitrack")]
        public static extern void StartServer(string OptitrackIp, string ListenIp, string UnityIp, string SaveFile, int LogLevel);
    }
}
