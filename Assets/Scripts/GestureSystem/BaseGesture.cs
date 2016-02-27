using UnityEngine;

namespace GestureControl
{
    public abstract class GestureBase : MonoBehaviour
    {
        public abstract string GetName();

        public abstract bool CheckConditions();



        public delegate void GestureEventHandler(GestureBase gesture, GestureEvent e);

        public event GestureEventHandler GestureStart;
        public event GestureEventHandler GestureHold;
        public event GestureEventHandler GestureEnd;

        protected void OnGestureStart(GestureEvent e)
        {
            if (GestureStart != null)
            {
                GestureStart(this, e);
            }
        }


        protected void OnGestureHold(GestureEvent e)
        {
            if (GestureHold != null)
            {
                GestureHold(this, e);
            }
        }


        protected void OnGestureEnd(GestureEvent e)
        {
            if (GestureEnd != null)
            {
                GestureEnd(this, e);
            }
        }





        protected void Start()
        {
            GestureSystem.RegisterGesture(this);
        }
    }
}
