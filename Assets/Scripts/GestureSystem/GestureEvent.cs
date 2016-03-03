using System;
using UnityEngine.Events;

namespace Assets.Scripts.GestureControl
{
    [Serializable]
    public class GestureEvent : UnityEvent<GestureEventArgs>
    {
    }
}
