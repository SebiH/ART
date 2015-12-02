using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
            Log.Add("> " + CurrentInput);

            string actualCommand = "";
            bool first = true;
            List<string> parameters = new List<string>();
            
            foreach (var parameter in CurrentInput.Split(' '))
            {
                if (first)
                {
                    first = false;
                    actualCommand = parameter;
                }
                else
                {
                    parameters.Add(parameter);
                }
            }

            Log.AddRange(UConsoleFactory.ExecuteCommand(actualCommand.Trim(), parameters));
            CurrentInput = "";
        }
    }
}
