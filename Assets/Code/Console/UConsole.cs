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

            Log.AddRange(UCommandRegister.ExecuteCommand(actualCommand.Trim(), parameters));
            CurrentInput = "";
        }


        public void AutocompleteCurrentCommand()
        {
            var matchingCommands = UCommandRegister.GetAvailableCommands().Where(cmd => cmd.StartsWith(CurrentInput));

            if (matchingCommands.Count() == 1)
            {
                CurrentInput = matchingCommands.First();
            }
            else if (matchingCommands.Count() > 1)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var cmd in matchingCommands)
                {
                    sb.AppendFormat("{0}\t", cmd);
                }
                Log.Add(sb.ToString());
            }
            else
            {
                Log.Add("No matching command found");
            }
        }
    }
}
