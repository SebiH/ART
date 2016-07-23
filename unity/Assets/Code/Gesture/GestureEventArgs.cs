using System;

namespace Assets.Scripts.GestureControl
{
    public class GestureEventArgs
    {
        public GestureBase Sender;

        public GestureEventArgs(GestureBase sender)
        {
            Sender = sender;
        }
    }
}
