using Assets.Code.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Code.Console
{
    static class UConsoleFactory
    {

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
