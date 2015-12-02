using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Console
{
    class UConsole
    {
        public List<string> Log
        {
            get; private set;
        }

        public string CurrentInput
        {
            get; set;
        }


        public UConsole()
        {
            Log = new List<string>();
        }


        public void ExecuteCommand()
        {
            Log.Add(CurrentInput);
            CurrentInput = "";
        }
    }
}
