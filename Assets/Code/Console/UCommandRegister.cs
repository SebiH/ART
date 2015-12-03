using System.Collections.Generic;
using System.Linq;

namespace Assets.Code.Console
{
    static class UCommandRegister
    {
        private static List<UConsoleCommand> knownCommands = new List<UConsoleCommand>();

        public static void RegisterCommand(UConsoleCommand cmd)
        {
            knownCommands.Add(cmd);
        }

        public static void DeregisterCommand(UConsoleCommand cmd)
        {
            knownCommands.Remove(cmd);
        }

        public static IEnumerable<string> ExecuteCommand(string cmd, IEnumerable<string> parameters)
        {
            var matchingCommands = knownCommands.Where(x => x.name == cmd);

            if (matchingCommands.Any())
            {
                var output = new List<string>();

                foreach (var ucmd in matchingCommands)
                {
                    output.Add(ucmd.execute(parameters));
                }

                return output;
            }
            else
            {
                return new []{ "Unknown command" };
            }
        }
    }
}
