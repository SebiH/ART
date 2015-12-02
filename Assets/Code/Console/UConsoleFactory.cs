using Assets.Code.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
