using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Console
{
    class UConsoleCommand
    {
        public Func<IEnumerable<string>, string> execute
        {
            get; private set;
        }

        public string name
        {
            get; private set;
        }


        public UConsoleCommand(string name, Func<IEnumerable<string>, string> action)
        {
            this.name = name;
            this.execute = action;
        }
    }
}
