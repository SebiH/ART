using UnityEngine;
using UnityEngine.UI;

namespace Assets.Modules.Menu
{
    public class UIFactory : MonoBehaviour
    {
        public GameObject ContainerTemplate;
        public GameObject ButtonTemplate;
        public GameObject SeparatorTemplate;

        public GameObject CreateContainer()
        {
            return Instantiate(ContainerTemplate);
        }


        public GameObject CreateButton(string text)
        {
            var button = Instantiate(ButtonTemplate);
            var uiElement = button.GetComponent<UIElement>();
            uiElement.SetText(text);
            return button;
        }

        public GameObject CreateSeparator()
        {
            return Instantiate(SeparatorTemplate);
        }
    }
}
