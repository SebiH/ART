using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Menu
{
    public class Menu : MonoBehaviour
    {
        private List<UIElement> _uiElements = new List<UIElement>();

        private MenuRenderer _renderer;
        private MenuInput _input;

        void OnEnable()
        {
            _input = GetComponent<MenuInput>();
            _input.OnButtonPress += OnInput;
            _renderer = GetComponent<MenuRenderer>();
            bool hasSelectedEntry = false;

            foreach (Transform child in transform)
            {
                var entry = child.GetComponent<UIElement>();
                _renderer.AddElement(entry);
                _uiElements.Add(entry);

                if (!hasSelectedEntry && entry.IsSelectable)
                {
                    SelectEntry(entry);
                    hasSelectedEntry = true;
                }
            }
        }


        void OnDisable()
        {

        }

        private void SelectEntry(UIElement entry)
        {
            foreach (var el in _uiElements)
            {
                el.SetHighlight(el == entry, Color.green);
            }
        }

        private void OnInput(InputType input)
        {
            Debug.Log("Received input " + input.ToString());
        }
    }
}
