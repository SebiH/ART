using System.Collections.Generic;
using UnityEngine;

namespace Assets.Modules.Menu
{
    public class Menu : MonoBehaviour
    {
        private List<UIElement> _uiElements = new List<UIElement>();

        private MenuRenderer _renderer;
        private MenuInput _input;

        private int _selectedEntryIndex = -1;

        void OnEnable()
        {
            _input = GetComponent<MenuInput>();
            _input.OnButtonPress += OnInput;
            _renderer = GetComponent<MenuRenderer>();
            bool hasSelectedEntry = false;
            var entryCounter = 0;

            foreach (Transform child in transform)
            {
                var entry = child.GetComponent<UIElement>();
                _renderer.AddElement(entry);
                _uiElements.Add(entry);

                if (!hasSelectedEntry && entry.IsSelectable)
                {
                    SelectEntry(entry);
                    hasSelectedEntry = true;
                    _selectedEntryIndex = entryCounter;
                }

                entryCounter++;
            }
        }


        void OnDisable()
        {

        }

        private void OnInput(InputType input)
        {
            switch (input)
            {
                case InputType.Start:
                    break;
                case InputType.Up:
                    SelectPrevEntry();
                    break;
                case InputType.Down:
                    SelectNextEntry();
                    break;
                case InputType.Confirm:
                    break;
                case InputType.Cancel:
                    break;
            }
        }

        private void SelectEntry(UIElement entry)
        {
            var indexCounter = 0;
            foreach (var el in _uiElements)
            {
                el.SetHighlight(el == entry, Color.green);

                if (el == entry)
                {
                    _selectedEntryIndex = indexCounter;
                }

                indexCounter++;
            }
        }

        private void SelectNextEntry()
        {
            var entryIndex = _selectedEntryIndex + 1;

            while (entryIndex < _uiElements.Count)
            {
                Debug.Log("Y");
                var el = _uiElements[entryIndex];

                if (_uiElements[entryIndex].IsSelectable)
                {
                    SelectEntry(el);
                    break;
                }

                entryIndex++;
            }
        }

        private void SelectPrevEntry()
        {
            var entryIndex = _selectedEntryIndex - 1;

            while (entryIndex >= 0)
            {
                Debug.Log("X");
                var el = _uiElements[entryIndex];

                if (_uiElements[entryIndex].IsSelectable)
                {
                    SelectEntry(el);
                    break;
                }

                entryIndex--;
            }
        }
    }
}
