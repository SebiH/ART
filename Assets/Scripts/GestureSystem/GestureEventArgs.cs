using System;

namespace GestureControl
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
