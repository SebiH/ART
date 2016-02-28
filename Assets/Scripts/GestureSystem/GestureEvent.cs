using System;
using UnityEngine.Events;

namespace GestureControl
{
    [Serializable]
    public class GestureEvent : UnityEvent<GestureEventArgs>
    {
    }
}
