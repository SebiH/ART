using UnityEngine;

namespace GestureControl
{
    public class RegisterLimb : MonoBehaviour
    {
        public InteractionLimb LimbType;

        protected void Start()
        {
            GestureSystem.RegisterLimb(LimbType, gameObject);
        }
    }
}
