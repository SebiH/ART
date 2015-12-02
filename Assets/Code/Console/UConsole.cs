using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Console
{
    class UConsole
    {
        public static List<string> Log
        {
            get; private set;
        }

        public static string CurrentInput
        {
            get; set;
        }


        public UConsole()
        {
            Log = new List<string>();
        }


        public void ExecuteCommand(string cmd)
        {
            Log.Add(cmd);
        }
    }
}
