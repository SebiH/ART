using UnityEngine;

namespace Assets.Modules.Menu
{
    public abstract class MenuEntry : MonoBehaviour
    {
        public abstract string GetName();
        public abstract void OnClicked();
    }
}
