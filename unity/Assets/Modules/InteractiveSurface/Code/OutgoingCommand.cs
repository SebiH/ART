using System;

namespace Assets.Modules.InteractiveSurface
{
    [Serializable]
    public class OutgoingCommand
    {
        public string command = "";
        public string target = "";
        public string payload = "";
    }
}
