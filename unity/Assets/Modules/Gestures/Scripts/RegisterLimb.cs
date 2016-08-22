using UnityEngine;

namespace Assets.Scripts.GestureControl
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
