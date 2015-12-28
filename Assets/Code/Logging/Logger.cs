using System;

namespace Assets.Code.Logging
{
    public static class Logger
    {
        public static void Info(string message, params object[] args)
        {
            var msg = String.Format(message, args);
            WriteLog("[INFO] " + msg);
        }


        public static void Debug(string message, params object[] args)
        {
            var msg = String.Format(message, args);
            WriteLog("[DEBUG] " + msg);
        }

        public static void Warning(string message, params object[] args)
        {
            var msg = String.Format(message, args);
            WriteLog("[WARNING] " + msg);
        }

        public static void Critical(string message, params object[] args)
        {
            var msg = String.Format(message, args);
            WriteLog("[CRITICAL] " + msg);
        }

        public static void Error(string message, params object[] args)
        {
            var msg = String.Format(message, args);
            WriteLog("[ERROR] " + msg);
        }

        public static void Log(string message, params object[] args)
        {
            var msg = String.Format(message, args);
            WriteLog("[LOG] " + msg);
        }




        private static void WriteLog(string msg)
        {
            // TODO: alternative logging, e.g. to file
            UnityEngine.Debug.Log(msg);
        }

    }
}
