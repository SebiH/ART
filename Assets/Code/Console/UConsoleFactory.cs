using Assets.Code.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    static class UConsoleFactory
    {
        private static List<UConsoleCommand> knownCommands = new List<UConsoleCommand>();

        public static void RegisterCommand(UConsoleCommand cmd)
        {
            knownCommands.Add(cmd);
        }

        public static void UnregisterCommand(UConsoleCommand cmd)
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

        // TODO: multiple consoles
        private static UConsole console;

        public static UConsole CreateConsole()
        {
            if (console == null)
            {
                console = new UConsole();
            }
            return console;
        }
    }
}
