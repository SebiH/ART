using System;

namespace Assets.Modules.InteractiveSurface
{
    [Serializable]
    public class IncomingCommand
    {
        public string command;
        public string origin;
        public string payload;
    }
}
