using UnityEngine;

namespace Assets.Scripts.Gesture
{
    public abstract class GestureBase : MonoBehaviour
    {
        public abstract string GetName();

        public abstract bool CheckConditions();



        protected void Start()
        {
            GestureSystem.RegisterGesture(this);
        }
    }
}
