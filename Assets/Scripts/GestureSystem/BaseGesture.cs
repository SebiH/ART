using UnityEngine;

namespace GestureControl
{
    public abstract class GestureBase : MonoBehaviour
    {
        public abstract string GetName();

        public abstract bool CheckConditions();


        public GestureEvent GestureStart;
        public GestureEvent GestureHold;
        public GestureEvent GestureEnd;

        protected void OnGestureStart()
        {
            if (GestureStart != null)
            {
                GestureStart.Invoke(new GestureEventArgs(this));
            }
        }


        protected void OnGestureHold()
        {
            if (GestureHold != null)
            {
                GestureHold.Invoke(new GestureEventArgs(this));
            }
        }


        protected void OnGestureEnd()
        {
            if (GestureEnd != null)
            {
                GestureEnd.Invoke(new GestureEventArgs(this));
            }
        }





        protected void Start()
        {
            GestureSystem.RegisterGesture(this);
        }
    }
}
