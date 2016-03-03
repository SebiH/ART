using UnityEngine;

namespace GestureControl
{
    public abstract class GestureBase : MonoBehaviour
    {
        public abstract string GetName();

        public abstract bool CheckConditions();


        public GestureEvent GestureStart;
        public GestureEvent GestureActive;
        public GestureEvent GestureStop;

        protected void RaiseGestureStartEvent()
        {
            if (GestureStart != null)
            {
                GestureStart.Invoke(new GestureEventArgs(this));
            }
        }


        protected void RaiseGestureActiveEvent()
        {
            if (GestureActive != null)
            {
                GestureActive.Invoke(new GestureEventArgs(this));
            }
        }


        protected void RaiseGestureStopEvent()
        {
            if (GestureStop != null)
            {
                GestureStop.Invoke(new GestureEventArgs(this));
            }
        }





        protected void Start()
        {
            GestureSystem.RegisterGesture(this);
        }
    }
}
